using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.ViewModels;
using Vintellitour_Framework.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Vintellitour_Framework.Controllers
{
    [Route("admin")]
    public class AdminAuthController : Controller
    {
        private readonly AdminAuthService _authService;

        public AdminAuthController(AdminAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View("~/Views/admin/login.cshtml");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View("~/Views/admin/login.cshtml", model);
            }

            var admin = await _authService.AuthenticateAsync(model.Email, model.Password);

            if (admin == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng hoặc bạn không có quyền truy cập.";
                return View("~/Views/admin/login.cshtml", model);
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, admin.Email),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.NameIdentifier, admin.Id)
        };

            var identity = new ClaimsIdentity(claims, "AdminAuthScheme");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("AdminAuthScheme", principal, new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
