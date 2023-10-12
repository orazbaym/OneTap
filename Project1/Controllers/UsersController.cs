using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project1.Services;
using Project1.ResourceModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Project1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly AppDbContext _authDbContext;

        public UsersController(UserManager<ApplicationUser> userManager, JwtService jwtService, SignInManager<ApplicationUser> signInManager, AppDbContext authDbContext)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _signInManager = signInManager;
            _authDbContext = authDbContext;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserModel model)
        {

            if (_authDbContext.Users.Any(x => x.UserName == model.Username))
            {
                ModelState.AddModelError("Username", "Пользователь с таким именем уже существует");
            }

            if (_authDbContext.Users.Any(x => x.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Пользавтель с такой почтой уже существует");
            }

            var user = new ApplicationUser
            {
                Firstname = model.FirstName,
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded && ModelState.IsValid)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);

                return Ok(user);
            }

            if (result.Succeeded)
            {
                await _userManager.DeleteAsync(user);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("Register", error.Description);
            }

            return BadRequest(ModelState);

            /*if(ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return Ok(user);
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ModelState.AddModelError(string.Empty, "Без успешная попытка регистрации");
            }
            return BadRequest(ModelState);*/
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<UserInfo>> GetLoggedInUser()
        {
            var currentUser = HttpContext.User;

            var name = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

            ApplicationUser user = await _userManager.FindByNameAsync(name);

            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }
            return new UserInfo
            {
                FirstName = user.Firstname,
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ImageUrl = user.ImageUrl
            };
        }

        [Authorize]
        [HttpPost("current")]
        public async Task<ActionResult<UserInfo>> UpdateCurrentUserInfo(UserUpdateModel model)
        {

            var currentUser = HttpContext.User;
            var name = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = currentUser.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByNameAsync(name);
            if (user != null)
            {

                if(email != model.Email)
                {
                    if (_authDbContext.Users.Any(x => x.Email == model.Email))
                    {
                        ModelState.AddModelError("Update", "Пользователь с такой почтой уже существует");
                    }
                }

                if(ModelState.IsValid)
                {
                    user.Firstname = model.FirstName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.ImageUrl = model.ImageUrl;
                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        return Ok(model);
                    }
                    return BadRequest(result);
                }
                return BadRequest(ModelState);

            }
            return NotFound();
            /*            var currentUser = HttpContext.User;

                        var name = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

                        var existingUser = await _authDbContext.Users.FindAsync(name);

                        if (existingUser != null)
                        {
                            if (_authDbContext.Users.Any(x => x.UserName == updatedUser.Username))
                            {
                                ModelState.AddModelError(string.Empty, "Username is already exists");
                            }

                            if (_authDbContext.Users.Any(x => x.Email == updatedUser.Email))
                            {
                                ModelState.AddModelError(string.Empty, "Email is already exists");
                            }
                            if (ModelState.IsValid)
                            {
                                existingUser.Firstname = updatedUser.FirstName;
                                existingUser.UserName = updatedUser.Username;
                                existingUser.Email = updatedUser.Email;
                                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                                existingUser.ImageUrl = updatedUser.ImageUrl;

                                await _authDbContext.SaveChangesAsync();
                                return Ok(existingUser);
                            }
                            return BadRequest(ModelState);
                        }
                        return NotFound();*/

        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login(AuthenticationRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, true, false);

                if (result.Succeeded)
                {
                    var token = _jwtService.CreateToken(user);
                    return Ok(token);
                }

                ModelState.AddModelError("Login", "Неправильный логин или пароль");

            }
            return BadRequest(ModelState);

        }
    }
}
