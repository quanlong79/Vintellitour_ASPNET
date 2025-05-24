using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO; // bổ sung
using System.Threading.Tasks;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;

public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: User/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest();

        var user = await _userService.GetUserIdAsync(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string id, string username, IFormFile avatar)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest();

        var user = await _userService.GetUserIdAsync(id);
        if (user == null)
            return NotFound();

        if (!string.IsNullOrEmpty(username))
            user.Username = username;

        if (avatar != null && avatar.Length > 0)
        {
            var avatarUrl = await SaveAvatarAsync(avatar);
            user.Avatar = avatarUrl;

            // Cập nhật avatar mới vào session
            HttpContext.Session.SetString("Avatar", avatarUrl);
        }

        await _userService.UpdateUserAsync(user);

        // Nếu username thay đổi, cũng nên cập nhật session luôn
        if (!string.IsNullOrEmpty(username))
        {
            HttpContext.Session.SetString("Username", username);
        }

        return RedirectToAction("Details", new { id });
    }


    private async Task<string> SaveAvatarAsync(IFormFile avatar)
    {
        // Lưu avatar vào wwwroot/uploads/avatars
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatar.FileName);
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars", fileName);

        // Tạo thư mục nếu chưa tồn tại
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        using (var stream = System.IO.File.Create(filePath))
        {
            await avatar.CopyToAsync(stream);
        }

        return "/uploads/avatars/" + fileName;
    }
}
