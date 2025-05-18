using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;

namespace Vintellitour_Framework.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        //private readonly MongoDbService _mongoDbService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        //public AccountController(IUserService userService, MongoDbService mongoDbService)
        //{
        //    _userService = userService;
        //    _mongoDbService = mongoDbService;
        //}


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
                    return View(model); // Trả lại view nếu mật khẩu không khớp
                }

                var user = await _userService.RegisterUserAsync(model.Username, model.Email, model.Password);
                if (user == null)
                {
                    ViewData["Error"] = "Email đã được đăng ký.";
                    return View(model); // Trả lại view nếu email đã có
                }

                return RedirectToAction("Login");  // Sau khi đăng ký thành công, chuyển hướng đến trang đăng nhập
            }

            // Trả về lại form nếu model không hợp lệ
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


        //public async Task<IActionResult> TestRawDb()
        //{
        //    var count = await _mongoDbService.GetRawUserCountAsync();
        //    return Content($"Số user raw từ MongoDbService: {count}");
        //}

    }
}
