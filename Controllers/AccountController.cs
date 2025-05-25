using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;

namespace Vintellitour_Framework.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        //private readonly MongoDbService _mongoDbService;
        private readonly IEmailSender _emailSender;

        // Cập nhật constructor để inject IEmailSender
        public AccountController(IUserService userService, IEmailSender emailSender)
        {
            _userService = userService;
            _emailSender = emailSender;
        }


        // Trang đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            return View();  // Trả về view đăng ký
        }

        // Xử lý đăng ký
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ViewData["Error"] = "Mật khẩu không khớp.";
                    return View(model);
                }

                var user = await _userService.RegisterUserAsync(model.Username, model.Email, model.Password, Request);
                if (user == null)
                {
                    ViewData["Error"] = "Email đã được đăng ký.";
                    return View(model);
                }

                TempData["Success"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.";
                return RedirectToAction("Login");
            }

            return View(model);
        }



        // Trang đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            return View();  // Trả về view đăng nhập
        }

        // Xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userService.LoginUserAsync(email, password);
            if (user == null)
            {
                ViewData["Error"] = "Sai email hoặc mật khẩu.";
                return View();
            }

            // Kiểm tra nếu tài khoản chưa xác thực
            if (!user.IsVerified)
            {
                ViewData["Error"] = "Tài khoản chưa được xác thực. Vui lòng kiểm tra email.";
                return View();
            }

            // Lưu thông tin user vào session
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Avatar", user.Avatar ?? "/img/default-avatar.png");

            // Sau khi đăng nhập thành công, có thể chuyển hướng tới trang chính
            return RedirectToAction("Index", "Home");

        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa session user
            return RedirectToAction("Login", "Account"); // Chuyển về trang Login của AccountController
        }


        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token xác thực không hợp lệ.");
            }

            bool verified = await _userService.VerifyUserAsync(token);
            if (verified)
            {
                // Xác thực thành công, chuyển về trang đăng nhập
                return RedirectToAction("Login", "Account", new { verified = true });
            }
            else
            {
                // Xác thực thất bại (token không đúng hoặc đã dùng)
                return View("VerificationFailed");
            }
        }
    }
}
