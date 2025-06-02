using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.ViewModels;
using System.Collections.Generic;
using System.Security.Claims;

public class OrderController : Controller
{
    private readonly PaymentService _paymentService;

    public OrderController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private string? GetUserId()
    {
        return HttpContext.Session.GetString("UserId");
    }

    // Xem lịch sử đơn hàng user hiện tại
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        List<Payment> userOrders = await _paymentService.GetByUserIdAsync(userId);
        return View("~/Views/User/Orders.cshtml", userOrders);
    }

}
