using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Models.DTOs;
using Vintellitour_Framework.Services;

namespace Vintellitour_Framework.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly PaymentService _paymentService;

        public PaymentController(IMomoService momoService, PaymentService paymentService)
        {
            _momoService = momoService;
            _paymentService = paymentService;
        }

        [HttpPost]
        [Route("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl([FromForm] PaymentRequestDto request)
        {
            var cartItems = JsonConvert.DeserializeObject<List<OrderItemDto>>(request.CartItemsJson);
            var userId = User.Identity.Name ?? "anonymous";

            var payment = await _paymentService.CreatePaymentAsync(
                userId,
                request.Amount,
                "Pending",
                cartItems.Select(i => new DetailPayment
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            );

            var momoResponse = await _momoService.CreatePaymentAsync(new OrderInfoModel
            {
                FullName = request.FullName,
                Amount = request.Amount,
                OrderInfo = request.OrderInfo,
                OrderId = payment.Id.ToString()  // chuyển ObjectId sang string
            });

            return Redirect(momoResponse.PayUrl);
        }
    }
}
