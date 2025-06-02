using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace Vintellitour_Framework.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly PaymentService _paymentService;
        private readonly IOptions<MomoOptionModel> _momoOptions;

        public CheckoutController(PaymentService paymentService, IOptions<MomoOptionModel> momoOptions)
        {
            _paymentService = paymentService;
            _momoOptions = momoOptions;
        }

        [HttpPost("/Checkout/MomoNotify")]
        public async Task<IActionResult> MomoNotify()
        {
            try
            {
                Console.WriteLine("=============== MoMo CALLBACK RECEIVED ===============");
                Console.WriteLine($"Timestamp: {DateTime.Now}");
                Console.WriteLine($"Content-Type: {Request.ContentType}");
                Console.WriteLine($"Method: {Request.Method}");
                Console.WriteLine($"Headers: {string.Join(", ", Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
                Console.WriteLine("====================================================");

                string requestBody = "";
                MoMoCallbackModel model = null;

                // Kiểm tra Content-Type và parse data tương ứng
                if (Request.ContentType?.Contains("application/json") == true)
                {
                    // Parse JSON
                    Request.Body.Position = 0;
                    using (var reader = new StreamReader(Request.Body))
                    {
                        requestBody = await reader.ReadToEndAsync();
                        Console.WriteLine($"JSON Body: {requestBody}");

                        if (!string.IsNullOrEmpty(requestBody))
                        {
                            model = JsonConvert.DeserializeObject<MoMoCallbackModel>(requestBody);
                        }
                    }
                }
                else
                {
                    // Parse Form data
                    model = new MoMoCallbackModel
                    {
                        PartnerCode = Request.Form["partnerCode"],
                        OrderId = Request.Form["orderId"],
                        RequestId = Request.Form["requestId"],
                        Amount = long.TryParse(Request.Form["amount"], out long amount) ? amount : 0,
                        OrderInfo = Request.Form["orderInfo"],
                        NoteOrder = Request.Form["orderInfo"],  // Gán thêm ở đây
                        OrderType = Request.Form["orderType"],
                        TransId = long.TryParse(Request.Form["transId"], out long transId) ? transId : 0,
                        ResultCode = int.TryParse(Request.Form["resultCode"], out int resultCode) ? resultCode : -1,
                        Message = Request.Form["message"],
                        PayType = Request.Form["payType"],
                        ResponseTime = long.TryParse(Request.Form["responseTime"], out long responseTime) ? responseTime : 0,
                        ExtraData = Request.Form["extraData"],
                        Signature = Request.Form["signature"]
                    };

                    // Log form data để debug
                    Console.WriteLine($"Form Data - OrderId: {model.OrderId}, ResultCode: {model.ResultCode}");
                }

                if (model == null || string.IsNullOrEmpty(model.OrderId))
                {
                    Console.WriteLine("Invalid notify data - model is null or OrderId is empty");
                    return BadRequest("Invalid notify data");
                }

                Console.WriteLine($"Processing MoMo callback - OrderId: {model.OrderId}, ResultCode: {model.ResultCode}, Amount: {model.Amount}");

                // Xác thực chữ ký
                if (!VerifyMomoSignature(model))
                {
                    Console.WriteLine($"Invalid signature for OrderId: {model.OrderId}");
                    return BadRequest("Invalid signature");
                }

                Console.WriteLine("Signature verified successfully");

                // Cập nhật trạng thái thanh toán
                string newStatus;
                if (model.ResultCode == 0)
                {
                    newStatus = "Success";
                    Console.WriteLine($"Payment successful for OrderId: {model.OrderId}");
                }
                else
                {
                    newStatus = "Cancel";
                    Console.WriteLine($"Payment failed/cancelled for OrderId: {model.OrderId}, ResultCode: {model.ResultCode}, Message: {model.Message}");
                }

                // Cập nhật database
                var updateResult = await _paymentService.UpdateStatusAsync(model.OrderId, newStatus, model.NoteOrder);
                Console.WriteLine($"Database update result for OrderId {model.OrderId}: {updateResult}");

                return Ok(new { message = "Callback processed successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing MoMo callback: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("/Checkout/PaymentCallBack")]
        public async Task<IActionResult> PaymentCallBack(string orderId)
        {
            var payment = await _paymentService.GetByIdAsync(orderId);
            return View("PaymentResult", payment);
        }

        // Thêm endpoint test để kiểm tra MoMo có thể gọi được không
        [HttpGet("/Test/Ping")]
        public IActionResult Ping()
        {
            Console.WriteLine($"Ping received at: {DateTime.Now}");
            return Ok(new { message = "Pong", timestamp = DateTime.Now });
        }

        [HttpPost("/Test/Ping")]
        public IActionResult PingPost()
        {
            Console.WriteLine($"POST Ping received at: {DateTime.Now}");
            return Ok(new { message = "Pong POST", timestamp = DateTime.Now });
        }

        // Test callback với fake data
        [HttpGet("/Test/FakeCallback")]
        public async Task<IActionResult> FakeCallback(string orderId = "6839bb05ce08b5fa6e1db5cd")
        {
            Console.WriteLine($"Fake callback for orderId: {orderId}");
            var result = await _paymentService.UpdateStatusAsync(orderId, "Success");
            return Json(new { success = result, orderId, message = "Fake callback executed" });
        }

        /// <summary>
        /// Xác thực chữ ký MoMo callback
        /// </summary>
        private bool VerifyMomoSignature(MoMoCallbackModel model)
        {
            try
            {
                // Tạo raw data theo thứ tự MoMo yêu cầu
                var rawData = $"partnerCode={model.PartnerCode}" +
                             $"&accessKey={_momoOptions.Value.AccessKey}" +
                             $"&requestId={model.RequestId}" +
                             $"&amount={model.Amount}" +
                             $"&orderId={model.OrderId}" +
                             $"&orderInfo={model.OrderInfo}" +
                             $"&orderType={model.OrderType}" +
                             $"&transId={model.TransId}" +
                             $"&message={model.Message}" +
                             $"&localMessage={model.Message}" +
                             $"&responseTime={model.ResponseTime}" +
                             $"&errorCode={model.ResultCode}" +
                             $"&payType={model.PayType}" +
                             $"&extraData={model.ExtraData}";

                Console.WriteLine($"Raw data for signature: {rawData}");

                // Tính toán chữ ký
                var computedSignature = ComputeHmacSha256(rawData, _momoOptions.Value.SecretKey);

                Console.WriteLine($"Computed signature: {computedSignature}");
                Console.WriteLine($"Received signature: {model.Signature}");

                // So sánh chữ ký
                bool isValid = computedSignature.Equals(model.Signature, StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"Signature validation result: {isValid}");

                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying signature: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tính toán HMAC SHA256
        /// </summary>
        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}