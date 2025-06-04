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
            return RedirectToAction("Index", "/");

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

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                ViewBag.Message = "Vui lòng nhập email.";
                ViewBag.IsError = true;
                return View();
            }

            // Tìm user theo email
            var user = await _userService.GetUserByEmailAsync(Email);
            if (user == null)
            {
                ViewBag.Message = "Email không tồn tại trong hệ thống.";
                ViewBag.IsError = true;
                return View();
            }

            // Tạo token reset password mới
            var resetToken = Guid.NewGuid().ToString();

            // Lưu token vào user và cập nhật trong DB
            user.ResetPasswordToken = resetToken;
            await _userService.UpdateUserAsync(user);

            // Tạo link reset password gửi cho user
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var resetLink = $"{baseUrl}/Account/ResetPassword?token={resetToken}";

            // Gửi email có giao diện đẹp cho việc đặt lại mật khẩu
            var emailBody = $@"
                <div style=""font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 30px;"">
                  <div style=""max-width: 600px; margin: auto; background: white; padding: 40px; border-radius: 10px; box-shadow: 0 4px 15px rgba(0,0,0,0.1);"">
                    <h2 style=""color: #1a73e8; text-align: center; margin-bottom: 30px;"">Đặt lại mật khẩu VintelliTour</h2>
                    <p style=""font-size: 16px; color: #333;"">Xin chào <strong>{user.Username}</strong>,</p>
                    <p style=""font-size: 16px; color: #333;"">
                      Vui lòng nhấn vào nút bên dưới để đặt lại mật khẩu của bạn.
                    </p>
                    <div style=""text-align: center; margin: 40px 0;"">
                      <a href=""{resetLink}"" 
                         style=""background-color: #1a73e8; color: white; padding: 15px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px; display: inline-block;""
                         target=""_blank""
                         rel=""noopener noreferrer"">
                        Đặt lại mật khẩu
                      </a>
                    </div>
                    <p style=""font-size: 14px; color: #666;"">
                      Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
                    </p>
                    <p style=""font-size: 14px; color: #666;"">Xin cảm ơn!</p>
                  </div>
                </div>
            ";

            await _emailSender.SendEmailAsync(user.Email, "Đặt lại mật khẩu VintelliTour", emailBody);

            ViewBag.Message = "Chúng tôi đã gửi liên kết đặt lại mật khẩu tới email của bạn. Vui lòng kiểm tra email.";
            ViewBag.IsError = false;
            return View();
        }



        // GET: /Account/ResetPassword?token=xxx
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Token không hợp lệ.");
                return View(new ResetPasswordModel());
            }

            var model = new ResetPasswordModel { Token = token };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            var user = await _userService.GetUserByResetTokenAsync(model.Token);

            if (user == null)
            {
                ModelState.AddModelError("", "Token đặt lại mật khẩu không hợp lệ.");
                return View(model);
            }

            // Bỏ phần kiểm tra hết hạn token

            // Cập nhật mật khẩu mới
            await _userService.ResetPasswordAsync(user, model.Password);

            TempData["Success"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập lại.";
            return RedirectToAction("Login");
        }
    }
}
