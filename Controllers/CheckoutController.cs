using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly PaymentService _paymentService;

        public CheckoutController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("/Checkout/MomoNotify")]
        public async Task<IActionResult> MomoNotify([FromForm] MomoCallbackModel model)
        {
            // TODO: xác thực chữ ký tại đây

            if (model.ResultCode == 0)
                await _paymentService.UpdateStatusAsync(model.OrderId, "Success");
            else
                await _paymentService.UpdateStatusAsync(model.OrderId, "Cancel");

            return Ok();
        }

        [HttpGet("/Checkout/PaymentCallBack")]
        public async Task<IActionResult> PaymentCallBack(string orderId)
        {
            var payment = await _paymentService.GetByIdAsync(orderId);
            return View("PaymentResult", payment); // trả về View kết quả
        }
    }
}
