using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Services;

namespace Vintellitour_Framework.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // Đăng ký người dùng (API)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return BadRequest(new { message = "Mật khẩu không khớp." });
            }

            var user = await _userService.RegisterUserAsync(registerDto.Username, registerDto.Email, registerDto.Password);
            if (user == null)
            {
                return BadRequest(new { message = "Email đã được đăng ký." });
            }

            return Ok(new { message = "Đăng ký thành công." });
        }

        // Đăng nhập người dùng (API)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userService.LoginUserAsync(loginDto.Email, loginDto.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Sai email hoặc mật khẩu." });
            }

            return Ok(new { message = "Đăng nhập thành công.", userId = user.Id });
        }
    }

    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
