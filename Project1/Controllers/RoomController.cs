using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project1.ResourceModels;

namespace Project1.Controllers
{
    [Authorize]
    [Route("api/ComputerClub")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{computerClubId}/rooms")]
        public IActionResult GetRooms([FromRoute] int computerClubId)
        {
            var rooms = _context.Rooms.Where(r => r.ComputerClubId == computerClubId).Include("Seats").ToList();
            if(rooms.IsNullOrEmpty())
                return NotFound(new {message = "В данном компьютерном клубе пока нет комнат"});

            return Ok(rooms);
        }

        [HttpGet("{computerClubId}/rooms/{roomId}")]
        public IActionResult GetRoom([FromRoute] int computerClubId, [FromRoute] int roomId)
        {
            var room = _context.Rooms.Where(c => c.ComputerClubId == computerClubId && c.Id == roomId).Include("Seats");
            if (!room.IsNullOrEmpty())
                return Ok(room);
            return NotFound(new { message = "Такой комнаты в данном компьюторном клубе не существует" });
        }

        [HttpPost("{computerClubId}/room")]
        public async Task<ActionResult<Room>> AddRoom([FromRoute] int computerClubId, [FromBody] RoomDto roomDto)
        {
            var computerClub = await _context.ComputerClubs.FindAsync(computerClubId);

            if (computerClub == null)
            {
                return NotFound();
            }

            var room = new Room
            {
                Id = SetRoomId(computerClubId),
                Name = roomDto.Name,
                Description = roomDto.Description,
                PricePerHour = roomDto.PricePerHour,
                Capacity = roomDto.Capacity,
                ComputerClub = computerClub
            };

/*            room.ComputerClub.Rooms = GetAllRooms(computerClubId);*/
            room.Seats = GetSeats(room.Id);
/*            var roomId = _context.Rooms.Find(computerClubId, room.Id);*/

            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();

            computerClub.PricePerHour = GetAveragePriceByComputerClubId(computerClubId);
            computerClub.OverallCapacity = GetOverallCapacity(computerClubId);
            await _context.SaveChangesAsync();

            return Ok(room);
        }

        private int SetRoomId(int computerClubId)
        {
            var lastId = 0;
            var rooms = _context.Rooms.Where(r => r.ComputerClubId == computerClubId).OrderBy(r => r.Id);
            if (rooms.Any())
            {
                lastId = rooms.Last().Id;
            }
            int id = lastId + 1;

            return id;
        }

        private double GetAveragePriceByComputerClubId(int id)
        {
            List<Room> rooms = _context.Rooms.Where(r => r.ComputerClubId == id).ToList();
            if (rooms.IsNullOrEmpty())
                return 0;
            var avgPrice = rooms.Average(r => r.PricePerHour);
            return Math.Round(avgPrice);
        }

        private int GetOverallCapacity(int id)
        {
            List<Room> rooms = _context.Rooms.Where(r => r.ComputerClubId == id).ToList();
            if (rooms.IsNullOrEmpty())
                return 0;
            int capacity = rooms.Sum(r => r.Capacity);
            return capacity;
        }

        private ICollection<Seat> GetSeats(int id)
        {
            var seats = _context.Seats.Where(s => s.RoomId == id).ToList();
            return seats;
        }

        private ICollection<Room> GetAllRooms(int id)
        {
            var rooms = _context.Rooms.Where(r => r.ComputerClubId == id).ToList();
            return rooms;
        }
    }
}
