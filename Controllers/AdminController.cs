using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.ViewModels;

namespace Vintellitour_Framework.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminUserService _adminUserService;
        private readonly DashboardService _dashboardService;  // Dùng trực tiếp DashboardService

        public AdminController(IAdminUserService adminUserService, DashboardService dashboardService)
        {
            _adminUserService = adminUserService;
            _dashboardService = dashboardService;
        }

        // Phần Dashboard đã chỉnh sửa, lấy dữ liệu đầy đủ và truyền ViewModel
        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel
            {
                ProvinceEngagements = await _dashboardService.GetTop5ProvincesByEngagementAsync(),
                UserPostStatus = await _dashboardService.GetUserPostStatusAsync(),
                PostStatsByMonth = await _dashboardService.GetPostStatsByMonthAsync(System.DateTime.Now.Year)
            };
            return View(model);
        }

        public IActionResult locations()
        {
            return View();
        }

        public IActionResult posts()
        {
            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _adminUserService.GetAllAsync();
            return View(users);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "UserId không được để trống" });

            var deleted = await _adminUserService.DeleteAsync(userId);
            if (!deleted)
                return Json(new { success = false, message = "Không tìm thấy user" });

            return Json(new { success = true, message = "Xóa user thành công" });
        }

        [HttpPost]
        public async Task<IActionResult> EditUser([FromBody] EditUserRequest request)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Email))
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

            var updated = await _adminUserService.UpdateAsync(request.Id, request.Name, request.Email);
            if (!updated)
                return Json(new { success = false, message = "Cập nhật thất bại hoặc user không tồn tại" });

            return Json(new { success = true, message = "Cập nhật thành công" });
        }
    }

    public class EditUserRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
