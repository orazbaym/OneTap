using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Project1.ResourceModels;
using System.Net.WebSockets;

namespace Project1.Controllers
{
    [Authorize]
    [Route("api/ComputerClub")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComboController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{computerClubId}/combos")]
        public IActionResult GetListOfCombos([FromRoute]int computerClubId)
        {
            var combos = _context.Combos.Where(c => c.ComputerClubId == computerClubId).ToList();
            if (_context.ComputerClubs.Any(c => c.Id == computerClubId))
            {
                if (combos.Any())
                {
                    return Ok(combos);
                }
                return NotFound(new { message = "В данном компьюторном клубе пока что нет комбо" });
            }
            return BadRequest(new { message = $"Компьютерный клуб с id:{computerClubId} не существует" });
        }

        [HttpPost("{computerClubId}/combo")]
        public async Task<IActionResult> AddCombo([FromRoute] int computerClubId, ComboDto comboDto)
        {
            var computerClub = _context.ComputerClubs.Find(computerClubId);
            if (computerClub is not null)
            {
                var combo = new Combo
                {
                    Id = SetComboId(computerClubId),
                    ComputerClubId = computerClubId,
                    Name = comboDto.Name,
                    Price = comboDto.Price,
                    AmountOfHours = comboDto.AmountOfHours
                };

                await _context.Combos.AddAsync(combo);
                await _context.SaveChangesAsync();

                return Ok(combo);
            }


            return BadRequest(new { message = $"Компьютерный клуб с id:{computerClubId} не существует" });
        }

        private int SetComboId(int computerClubId)
        {
            var lastId = 0;
            var combos = _context.Combos.Where(c => c.ComputerClubId == computerClubId).OrderBy(c => c.Id);
            if (combos.Any())
            {
                lastId = combos.Last().Id;
            }
            int id = lastId + 1;

            return id;
        }
    }
}
