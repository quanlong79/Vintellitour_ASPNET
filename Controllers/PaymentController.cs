using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Models;


namespace Vintellitour_Framework.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;
        //private readonly IVnPayService _vnPayService;
        public PaymentController(IMomoService momoService)
        {
            _momoService = momoService;

        }
        [HttpPost]
        [Route("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
        }

    }
}
