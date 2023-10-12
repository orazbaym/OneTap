using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project1.Models;
using Project1.ResourceModels;

namespace Project1.Controllers
{
    [Authorize]
    [Route("api/ComputerClub")]
    [ApiController]
    public class SeatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{computerClubId}/seats")]
        public async Task<ActionResult<List<Seat>>> GetSeats3([FromRoute] int computerClubId, bool? available = null)
        {
            var computerClub = await _context.ComputerClubs.FindAsync(computerClubId);

            if (computerClub != null)
            {
                var seats = await _context.Seats.Where(s => s.RoomComputerClubId == computerClubId).ToListAsync();
                if (seats.Any())
                {
                    return available is null ? Ok(seats) : available is true ? Ok(seats.Where(s => s.AvailabilityStatus == "Available").ToList()) : Ok(seats.Where(s => s.AvailabilityStatus != "Available").ToList());
                }
                return NotFound(new { message = "В данном компьютерном клубе пока нет мест" });
            }
            return NotFound(new { message = "Такой компьютерный клуб не существует" });

        }

        [HttpGet("{computerClubId}/rooms/{roomId}/seats")]
        public IActionResult GetSeats2([FromRoute] int computerClubId, [FromRoute] int roomId)
        {
            var computerClub = _context.ComputerClubs.Find(computerClubId);
            if(computerClub == null)
                return NotFound(new {message = "Такой компьютерный клуб не существует"});

            var room = _context.Rooms.Find(computerClubId, roomId);
            if (room == null)
                return NotFound(new {message = "Такой комнаты в данном компьютерном клубе не существует"});

            var seats = _context.Seats.Where(s => s.RoomId == roomId && s.RoomComputerClubId == computerClubId).ToList();
            if (!seats.Any())
                return NotFound(new { message = "Место по вашим параметрам не было найдено" });
            return Ok(seats);

/*            var computerClub = _context.ComputerClubs.Find(computerClubId);
            var room = _context.Rooms.Find(roomId);
            var seats = _context.Seats.Where(s => s.RoomId == roomId && s.Room.ComputerClubId == computerClubId).ToList();

            return computerClub == null ?
                NotFound(new { message = "Такой компьютерный клуб не существует" }) :
                room == null ?
                NotFound(new { message = "Такой комнаты в данном компьютерном клубе не существует" }) :
                Ok(seats);*/
        }

        // Post api/ComputerClub/{computerClubId}/room/{roomId}/seats    Using this we can convert our seatId and roomId primary keys to foreign key
        [HttpPost("{computerClubId}/rooms/{roomId}/seat")]
        public async Task<ActionResult<Seat>> PostSeat2([FromRoute] int computerClubId, [FromRoute] int roomId, int numberOfSeatsToAdd)
        {   
            var room = await _context.Rooms.Where(r => r.ComputerClubId == computerClubId && r.Id == roomId).ToListAsync();

            if (!room.Any())
            {
                return NotFound(new {message = "Комната не найдена"});
            }
            Seat seat;
            List<Seat> addedSeats = new List<Seat>();
            for (int i = 0; i < numberOfSeatsToAdd; i++)
            {
                var numberOfSeatsInDb = _context.Seats.Where(s => s.RoomComputerClubId == computerClubId).Count();
                seat = new Seat
                {
                    Id = SetSeatId(computerClubId),
                    Name = $"No {numberOfSeatsInDb + 1}",
                    AvailabilityStatus = "Available"
                };

                _context.Seats.Add(seat);
                addedSeats.Add(seat);
                await _context.SaveChangesAsync();
            }

            return Ok(addedSeats);
        }

        private int SetSeatId(int computerClubId)
        {
            var lastId = 0;
            var seats = _context.Seats.Where(s => s.RoomComputerClubId == computerClubId).OrderBy(s => s.Id);
            if(seats.Any())
            {
                lastId = seats.Last().Id;
            }
            int id = lastId + 1;

            return id;
        }

    }
}
