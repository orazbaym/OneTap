using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Project1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        [HttpGet("GreetingMessage")]
        public IActionResult GetGreetingMessage() 
        {
            return Ok(new { message = "Мы запускаемся, спасибо что выбрали нас!",
                            title = "Чат с One Tap",
                            date = DateTime.Now.ToString("d")});
        }
        [HttpGet("NewsMessage")]
        public IActionResult GetNewsMessage()
        {
            return Ok(new { message = "Срочные новости, Cyber S Club наши официальные партнеры.",
                            title = "Новости",
                            date = DateTime.Now.ToString("d") });
        }
        [HttpGet("SaleMessage")]
        public IActionResult GetSaleMessage()
        {
            return Ok(new
            {
                message = "АКЦИЯ, первым 100 пользователям 50% акции!!!",
                title = "Акции",
                date = DateTime.Now.ToString("d")
            });
        }
    }
}
