using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project1.ResourceModels;
using Project1.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Project1.Controllers
{
    [Authorize]
    [Route("api/ComputerClub")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceManagement _serviceManagement;

        public BookingController(AppDbContext context, UserManager<ApplicationUser> userManager, IServiceManagement serviceManagement)
        {
            _context = context;
            _userManager = userManager;
            _serviceManagement = serviceManagement;
        }

        [HttpGet("myBookings")]
        public async Task<ActionResult> GetBookingsOfLoginnedUser() 
        {
            var currentUser = HttpContext.User;

            var name = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

            ApplicationUser user = await _userManager.FindByNameAsync(name);

            var userId = user.Id;
            var userBookings = await _context.Bookings.Where(b => b.UserId == userId).Include(b => b.Seat).Include(b => b.Seat.Room).Include(b => b.Seat.Room.ComputerClub).ToListAsync();
            return Ok(userBookings);
        }

        [HttpGet("{computerClubId}/booking/seats")]
        public async Task<IActionResult> SearchSeats([FromRoute]int computerClubId, [Required]int roomId, int comboId, [Required]bool isCombo, [Required]DateTime startTime, DateTime endTime, [Required]int numberOfSeats)
        {
            var computerClub =  await _context.ComputerClubs.FindAsync(computerClubId);
            if(computerClub is null)
            {
                return BadRequest(new { message = $"Такой компьютерный клуб не существует" });
            }

            double duration;
            double price;
            if (isCombo)
            {
                var combo = _context.Combos.Find(computerClubId, comboId);
                if(combo is null)
                    return BadRequest(new { message = $"Комбо с id:{comboId} не существует" });
                duration = combo.AmountOfHours;
                price = combo.Price;
                endTime = startTime.AddHours(duration);
            }
            else
            {
                if (endTime < startTime)
                    return BadRequest(new { message = $"Введите правильное время" });
                duration = (endTime - startTime).TotalHours;
                var room = _context.Rooms.Find(computerClubId, roomId);
                
                if(room is null)
                    return BadRequest(new { message = $"Зал с id:{roomId} не существует" });
                price = room.PricePerHour * duration;
            }

            var seats = await _context.Seats.Where(s => s.RoomComputerClubId == computerClubId && s.RoomId == roomId).Include(s => s.Room).ToListAsync();

            if (seats.Any())
            {
                
                var overlappingBookings = _context.Bookings
                    .Where(b => b.StartTime < endTime && b.EndTime > startTime && b.SeatRoomComputerClubId == computerClubId && b.SeatRoomId == roomId)
                    .ToList();
                var bookedSeats = new Dictionary<int, bool>();

                foreach (var seat in seats)
                {
                    // Check if the seat is booked during the requested time frame
                    var isBooked = overlappingBookings.Any(b => b.SeatId == seat.Id);

                    // Add the seat to the dictionary of booked seats
                    bookedSeats[seat.Id] = isBooked;
                }

                var result = seats.Select(s => new
                {
                    s.Id,
                    s.Name,
                    RoomName = s.Room.Name,
                    StartTime = startTime,
                    EndTime = endTime,
                    PricePerSeat = price,
                    FullPrice = price * numberOfSeats,
                    IsBooked = bookedSeats.ContainsKey(s.Id) ? bookedSeats[s.Id] : false
                }).ToList();

                return Ok(result);
            }
            return BadRequest(new { message = $"Мест по данному запросу не существует" });
        }
        

        [HttpPost("{computerClubId}/seats/{seatId}/booking")]
        public async Task<ActionResult> BookSeat(int computerClubId, int seatId, DateTime startTime, DateTime endTime)
        {
            var computerClub = await _context.ComputerClubs.FindAsync(computerClubId);
            var seat = await _context.Seats.Include(s => s.Bookings).FirstOrDefaultAsync(s => s.Id == seatId && s.RoomComputerClubId == computerClubId);
            if (seat == null)
            {
                return NotFound();
            }

            // Check if the seat is available during the requested time slot
            foreach (var booking in seat.Bookings)
            {
                if (startTime < booking.EndTime && booking.StartTime < endTime)
                {
                    return BadRequest($"Seat is not available during the requested time slot. It will be available from {booking.EndTime}.");
                }
            }
            var currentUser = HttpContext.User;

            var name = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

            ApplicationUser user = await _userManager.FindByNameAsync(name);
            var userId = user.Id;
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Asia Standard Time");
            // Book the seat
            var bookingToAdd = new Booking
            {
                UserId = userId,
                SeatId = seatId,
                StartTime = startTime,
                EndTime = endTime,
                Status = "Confirmed",
                CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, targetTimeZone)
            };

            seat.Bookings.Add(bookingToAdd);
            seat.AvailabilityStatus = "Booked";
            seat.BookingTimestamp = endTime;
            
            await _context.SaveChangesAsync();

            string jobId = BackgroundJob.Schedule(() =>
                _serviceManagement.UpdateExpiredBookings(), endTime);
                

/*            var bookingMessage = new BookingDto
            {
                Username = name,
                ComputerClubName = computerClub.Name,
                RoomName = seat.Room.Name,
                SeatName = seat.Name,
                StartTime = startTime,
                EndTime = endTime,
                CreatedTime= DateTime.UtcNow
            };*/

            return Ok(new { message = $"Место с  id:{seatId}, успешно забронирован" });
        }
        [HttpPost("{computerClubId}/booking/seats")]
        public async Task<ActionResult> BookSeats([Required]int computerClubId, [Required]int roomId, int comboId, [Required]bool isCombo, [Required]List<int> seatIds, [Required] DateTime startTime, DateTime endTime)
        {
            var computerClub = await _context.ComputerClubs.FindAsync(computerClubId);
            if (computerClub is null)
                return BadRequest(new { message = $"Компьютерный клуб с id:{computerClubId} не существует" });

            var currentUser = HttpContext.User;
            var name = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser user = await _userManager.FindByNameAsync(name);
            var userId = user.Id;
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Asia Standard Time");
            List<string> seatNames = new();

            foreach (var seatId in seatIds)
            {
                var seat = await _context.Seats.Include(s => s.Bookings)
                    .FirstOrDefaultAsync(s => s.Id == seatId && s.RoomComputerClubId == computerClubId);

                if (seat == null)
                {
                    return NotFound(new {message = $"Место с id:{seatId} не существует"});
                }

                double price;
                double duration;
                if (isCombo)
                {
                    var combo = await _context.Combos.FindAsync(computerClubId, comboId);
                    if (combo is null)
                        return BadRequest(new { message = $"Комбо с id:{comboId} не существует" });
                    endTime = startTime.AddHours(combo.AmountOfHours);
                    price = combo.Price;
                }
                else
                {
                    if(endTime < startTime)
                        return BadRequest(new { message = $"Введите правильное время" });
                    var room = await _context.Rooms.FindAsync(computerClubId, roomId);
                    if (room is null)
                        return BadRequest(new { message = $"Зал с id:{roomId} не существует" });
                    duration = (endTime - startTime).TotalHours;
                    price = room.PricePerHour * duration;
                }

                if (!IsSeatAvailable(seat, startTime, endTime))
                {
                    var nextAvailableTime = GetNextAvailableTime(seat, startTime, endTime);
                    return BadRequest(new { message = $"Место с номером:{seat.Name} в данный момент не свободно. Оно будет доступно с {nextAvailableTime}." });
                }
                // Book the seat
                var bookingToAdd = new Booking
                {
                    UserId = userId,
                    SeatId = seatId,
                    StartTime = startTime,
                    EndTime = endTime,
                    PriceAmount = price,
                    IsCombo = isCombo,
                    Status = "Confirmed",
                    CreatedDate = TimeZoneInfo.ConvertTime(DateTime.Now, targetTimeZone)
                };

                seat.Bookings.Add(bookingToAdd);
                seat.AvailabilityStatus = "Booked";
                seat.BookingTimestamp = endTime;

                seatNames.Add(seat.Name);
            }

            await _context.SaveChangesAsync();

            string jobId = BackgroundJob.Schedule(() =>
                _serviceManagement.UpdateExpiredBookings(), endTime);

            return Ok(new {message = $"Места с  номером:{string.Join(", ", seatNames)}, успешно забронированы"});
        }

        private bool IsSeatAvailable(Seat seat, DateTime startTime, DateTime endTime)
        {
            foreach (var booking in seat.Bookings)
            {
                if (startTime < booking.EndTime && booking.StartTime < endTime)
                {
                    return false;
                }
            }

            return true;
        }

        private DateTime GetNextAvailableTime(Seat seat, DateTime startTime, DateTime endTime)
        {
            var nextAvailableTime = DateTime.MaxValue;

            foreach (var booking in seat.Bookings)
            {
                if (booking.EndTime > startTime && booking.EndTime < nextAvailableTime)
                {
                    nextAvailableTime = booking.EndTime;
                }
            }

            return nextAvailableTime;
        }
    }
}
