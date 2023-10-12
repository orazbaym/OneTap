 using GeoCoordinatePortable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project1.Pagination;
using Project1.Params;
using Project1.ResourceModels;
using System.Text.Json;

namespace Project1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ComputerClubController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComputerClubController(AppDbContext dataContext)
        {
            _context = dataContext;
        }

        [HttpGet("offset-pagination")]
        public async Task<IActionResult> GetComputerClubs([FromQuery] PaginationParams @params, [FromQuery] FilterParams @filterParams)
        {
            var datalist = await _context.ComputerClubs.Include("Rooms").Include("Combos").OrderBy(c => c.Id).ToListAsync();
            if (datalist.IsNullOrEmpty())
                return NotFound(new {message = "База данных пуста"});

            List<Func<IEnumerable<ComputerClub>, IEnumerable<ComputerClub>>> sortFunctions = new();
            if (filterParams.ByRating)
            {
                sortFunctions.Add(datalist => datalist.OrderByDescending(d => d.Rating));
            }
            if (filterParams.ByNearest)
            {
                var myLocation = new GeoCoordinate(filterParams.MyLatitude, filterParams.MyLongitude);

                var filteredByDistance = datalist
                    .Where(o => (new GeoCoordinate(o.Latitude, o.Longitude)
                    .GetDistanceTo(myLocation) / 1000) <= filterParams.MaxDistance);

                sortFunctions
                        .Add(datalist => filteredByDistance
                        .OrderBy(o => new GeoCoordinate(o.Latitude, o.Longitude)
                        .GetDistanceTo(myLocation)));
            }
            if (filterParams.By24x7)
            {
                sortFunctions.Add(datalist => datalist.Where(d => d.Is24x7));
            }
/*            if (filterParams.ByRatingDesc.HasValue) 
            {
                sortFunctions
                    .Add(datalist => filterParams
                    .ByRatingDesc == true ? 
                    datalist.OrderByDescending(d => d.Rating) : 
                    datalist.OrderBy(d => d.Rating));
            }

            if (filterParams.ByNearestAsc.HasValue)
            {
                var myLocation = new GeoCoordinate(filterParams.MyLatitude, filterParams.MyLongitude);

                var filteredByDistance = datalist
                    .Where(o => (new GeoCoordinate(o.Latitude, o.Longitude)
                    .GetDistanceTo(myLocation) / 1000) <= filterParams.MaxDistance);

                if(filterParams.ByNearestAsc == true)
                    sortFunctions
                        .Add(datalist => filteredByDistance
                        .OrderBy(o => new GeoCoordinate(o.Latitude, o.Longitude)
                        .GetDistanceTo(myLocation)));
                else
                    sortFunctions
                        .Add(datalist => filteredByDistance
                        .OrderByDescending(o => new GeoCoordinate(o.Latitude, o.Longitude)
                        .GetDistanceTo(myLocation)));
            }

            if (filterParams.ByIs24x7.HasValue)
            {
                sortFunctions.Add(datalist => datalist.Where(d => filterParams.ByIs24x7 == true ? d.Is24x7 : !d.Is24x7));
            }*/

            foreach (var sortFunction in sortFunctions)
                datalist = sortFunction(datalist).ToList();

            if (datalist.IsNullOrEmpty())
                return NotFound(new { message = "По вашему запросу компьютерные клубы не найдены" });

            var paginationMetadata = new PaginationMetadata(datalist.Count(), @params.Page, @params.ItemsPerPage);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(datalist.Skip((@params.Page - 1) * @params.ItemsPerPage)
                .Take(@params.ItemsPerPage));
        }

        [HttpGet]
        public IActionResult GetComputerClubs()
        {
            var computerClubs = _context.ComputerClubs.Include("Rooms").Include("Combos").ToList();
            return Ok(computerClubs);
        }

        [HttpPost]
        public async Task<IActionResult> AddComputerClub(ComputerClubDto[] computerClubDtos)
        {
            List<ComputerClub> addedComputerClubs = new();
            foreach(ComputerClubDto computerClubDto in computerClubDtos)
            {
                var computerClub = new ComputerClub()
                {
                    Name = computerClubDto.Name,
                    Rating = computerClubDto.Rating,
                    ReviewNumbers = computerClubDto.ReviewNumbers,
                    Description = computerClubDto.Description,
                    Latitude = computerClubDto.Latitude,
                    Longitude = computerClubDto.Longitude,
                    Address = computerClubDto.Address,
                    Imageurl = computerClubDto.Imageurl,
                    Is24x7 = computerClubDto.Is24x7
                };
                _context.ComputerClubs.Add(computerClub);
                addedComputerClubs.Add(computerClub);
            }
            await _context.SaveChangesAsync();

            return Ok(addedComputerClubs);

/*            var computerClub = new ComputerClub()
            {
                Name = addComputerClubRequest.Name,
                Rating = addComputerClubRequest.Rating,
                ReviewNumbers = addComputerClubRequest.ReviewNumbers,
                Description = addComputerClubRequest.Description,
                Latitude = addComputerClubRequest.Latitude,
                Longitude = addComputerClubRequest.Longitude,
                Address = addComputerClubRequest.Address,
                Imageurl = addComputerClubRequest.Imageurl,
                Is24x7 = addComputerClubRequest.Is24x7
            };
            await _context.ComputerClubs.AddAsync(computerClub);
            await _context.SaveChangesAsync();

            return Ok(computerClub);*/
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateComputerClub([FromRoute] int id, ComputerClubDto updateComputerClubRequest)
        {
            var computerClub = await _context.ComputerClubs.FindAsync(id);

            if (computerClub != null)
            {
                computerClub.Name= updateComputerClubRequest.Name;
                computerClub.Rating = updateComputerClubRequest.Rating;
                computerClub.ReviewNumbers = updateComputerClubRequest.ReviewNumbers;
                computerClub.Description = updateComputerClubRequest.Description;
                computerClub.Latitude = updateComputerClubRequest.Latitude;
                computerClub.Longitude = updateComputerClubRequest.Longitude;
                computerClub.Address = updateComputerClubRequest.Address;
                computerClub.Imageurl = updateComputerClubRequest.Imageurl;
                computerClub.Is24x7 = updateComputerClubRequest.Is24x7;

                await _context.SaveChangesAsync();

                return Ok(computerClub);
            }

            return NotFound("Компьютерный клуб не найден");
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetComputerClub([FromRoute] int id)
        {
            var computerClub = await _context.ComputerClubs.FindAsync(id);

            if(computerClub != null)
            {
                return Ok(computerClub);
            }

            return NotFound("Компьютерный клуб не найден");
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteComputerClub([FromRoute] int id)
        {
            var computerClub = await _context.ComputerClubs.FindAsync(id);

            if(computerClub != null)
            {
                _context.Remove(computerClub);
                await _context.SaveChangesAsync();
                return Ok($"Успешно удалено; Название:{computerClub.Name}, Id:{computerClub.Id}");
            }

            return NotFound("Компьютерный клуб не найден");
        }

        

        // POST api/seat/{id}/book
/*        [HttpPost("{id}/book")]
        public async Task<ActionResult<Booking>> BookSeat(int id, [FromBody] Booking booking)
        {
            var seat = await _context.Seats.FindAsync(id);

            if (seat == null)
            {
                return NotFound();
            }

            booking.SeatId = seat.Id;
            booking.Status = "Booked";
            booking.CreatedDate = DateTime.Now;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }*/

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.Id == id);
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }


    }

}
