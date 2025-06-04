using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Vintellitour_Framework.Controllers.Admin
{
    [Route("admin")]
    public class AdminOrderController : Controller
    {
        private readonly PaymentService _paymentService;

        public AdminOrderController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> Orders()
        {
            List<Payment> payments = await _paymentService.GetAllPaymentsAsync();
            return View("~/Views/admin/orders.cshtml", payments);
        }

        // API trả JSON danh sách đơn hàng (nếu cần ajax)
        [HttpGet("list")]
        public async Task<IActionResult> GetOrders()
        {
            List<Payment> payments = await _paymentService.GetAllPaymentsAsync();
            return Json(payments);
        }

        // Cập nhật trạng thái vận chuyển (ví dụ POST)
        [HttpPost("update-shipping")]
        public async Task<IActionResult> UpdateShippingStatus(string paymentId, string shippingStatus)
        {
            bool updated = await _paymentService.UpdateShippingStatusAsync(paymentId, shippingStatus);
            if (updated)
                return Ok(new { message = "Cập nhật thành công" });
            else
                return BadRequest(new { message = "Cập nhật thất bại" });
        }
    }
}
