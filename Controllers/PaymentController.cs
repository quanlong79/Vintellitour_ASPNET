using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Models.DTOs;
using Vintellitour_Framework.Services;
using System.Security.Cryptography;
using System.Text;

namespace Vintellitour_Framework.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly PaymentService _paymentService;
        private readonly CartService _cartService; // Add this if you need to clear cart

        public PaymentController(IMomoService momoService, PaymentService paymentService, CartService cartService = null)
        {
            _momoService = momoService;
            _paymentService = paymentService;
            _cartService = cartService;
        }


        [HttpPost]
        [Route("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl([FromForm] PaymentRequestDto request)
        {
            try
            {
                var cartItems = JsonConvert.DeserializeObject<List<OrderItemDto>>(request.CartItemsJson);
                var userId = User.Identity.Name ?? HttpContext.Session.GetString("UserId") ?? "anonymous";

                // Gộp OrderInfo với OrderNote nếu có
                var fullOrderInfo = request.OrderInfo ?? "";
                if (!string.IsNullOrWhiteSpace(request.OrderNote))
                {
                    fullOrderInfo += "\nGhi chú khách hàng: " + request.OrderNote.Trim();
                }

                Console.WriteLine($"Creating payment for user: {userId}, amount: {request.Amount}");

                // Tạo payment
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

                string paymentIdString = payment.Id.ToString();
                Console.WriteLine($"Created payment with ID: {paymentIdString}");

                // Tạo MoMo payment request, dùng fullOrderInfo thay vì request.OrderInfo
                var momoResponse = await _momoService.CreatePaymentAsync(new OrderInfoModel
                {
                    FullName = request.FullName,
                    Amount = request.Amount,
                    OrderInfo = fullOrderInfo,
                    OrderId = paymentIdString
                });

                Console.WriteLine($"MoMo response - PayUrl: {momoResponse.PayUrl}, OrderId: {momoResponse.OrderId}");

                if (string.IsNullOrEmpty(momoResponse.PayUrl))
                {
                    Console.WriteLine("MoMo PayUrl is empty!");
                    await _paymentService.UpdateStatusAsync(paymentIdString, "Cancel");
                    return BadRequest("Cancel to create payment URL");
                }

                return Redirect(momoResponse.PayUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating payment: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
        }


        // MoMo IPN (Instant Payment Notification) Callback
        [HttpPost]
        [Route("Payment/MoMoCallback")]
        public async Task<IActionResult> MoMoCallback()
        {
            try
            {
                Console.WriteLine("=== MoMo Callback Received ===");

                // Read the request body
                string requestBody;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                Console.WriteLine($"Callback Request Body: {requestBody}");
                Console.WriteLine($"Content-Type: {Request.ContentType}");

                // Log all headers
                foreach (var header in Request.Headers)
                {
                    Console.WriteLine($"Header {header.Key}: {header.Value}");
                }

                // Parse the callback data
                var callbackData = JsonConvert.DeserializeObject<MoMoCallbackModel>(requestBody);

                if (callbackData == null)
                {
                    Console.WriteLine("Cancel to parse callback data");
                    return BadRequest("Invalid callback data");
                }

                Console.WriteLine($"Parsed callback:");
                Console.WriteLine($"- OrderId: {callbackData.OrderId}");
                Console.WriteLine($"- ResultCode: {callbackData.ResultCode}");
                Console.WriteLine($"- Message: {callbackData.Message}");
                Console.WriteLine($"- Amount: {callbackData.Amount}");
                Console.WriteLine($"- TransId: {callbackData.TransId}");

                // Update payment status based on result code
                var newStatus = callbackData.ResultCode == 0 ? "Success" : "Cancel";
                var updateResult = await _paymentService.UpdateStatusAsync(callbackData.OrderId, newStatus, callbackData.OrderInfo);

                Console.WriteLine($"Payment status updated: {updateResult}, Status: {newStatus}");

                // Return success response to MoMo
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing MoMo callback: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
        }

        // MoMo Return URL (when user returns from MoMo page)
        [HttpGet]
        [Route("Payment/MoMoReturn")]
        public async Task<IActionResult> MoMoReturn(string partnerCode, string orderId, string requestId, long amount, string orderInfo, string orderType, long transId, int resultCode, string message, string payType, long responseTime, string extraData, string signature)
        {
            Console.WriteLine($"=== MoMo Return ===");
            Console.WriteLine($"OrderId: {orderId}");
            Console.WriteLine($"ResultCode: {resultCode}");
            Console.WriteLine($"Message: {message}");
            Console.WriteLine($"Amount: {amount}");
            Console.WriteLine($"TransId: {transId}");

            try
            {
                var payment = await _paymentService.GetByIdAsync(orderId);

                if (payment == null)
                {
                    TempData["Message"] = "Không tìm thấy đơn hàng.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index", "Cart");
                }

                if (resultCode == 0)
                {
                    // Success
                    await _paymentService.UpdateStatusAsync(orderId, "Success", orderInfo);
                    TempData["Message"] = "Thanh toán thành công!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    // Cancel
                    await _paymentService.UpdateStatusAsync(orderId, "Cancel", orderInfo);
                    TempData["Message"] = $"Thanh toán thất bại: {message}";
                    TempData["MessageType"] = "error";
                }

                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing MoMo return: {ex.Message}");
                TempData["Message"] = "Có lỗi xảy ra khi xử lý thanh toán.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Cart");
            }
        }

        // Method to verify MoMo signature
        private bool VerifyMoMoSignature(MoMoCallbackModel callback)
        {
            try
            {
                // You need to implement this based on your MoMo configuration
                // This is a placeholder - replace with actual signature verification logic

                var secretKey = "YOUR_MOMO_SECRET_KEY"; // Get this from your config

                // Build the signature string according to MoMo documentation
                var signatureString = $"amount={callback.Amount}&extraData={callback.ExtraData}&message={callback.Message}&orderId={callback.OrderId}&orderInfo={callback.OrderInfo}&orderType={callback.OrderType}&partnerCode={callback.PartnerCode}&payType={callback.PayType}&requestId={callback.RequestId}&responseTime={callback.ResponseTime}&resultCode={callback.ResultCode}&transId={callback.TransId}";

                var computedSignature = ComputeHmacSha256(signatureString, secretKey);

                Console.WriteLine($"Expected signature: {callback.Signature}");
                Console.WriteLine($"Computed signature: {computedSignature}");

                return computedSignature.Equals(callback.Signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying signature: {ex.Message}");
                return false;
            }
        }

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

        // Debug methods
        //[HttpGet]
        //[Route("Debug/Payments")]
        //public async Task<IActionResult> DebugPayments()
        //{
        //    var payments = await _paymentService.GetAllPaymentsAsync();
        //    return Json(payments.Select(p => new
        //    {
        //        Id = p.Id.ToString(),
        //        Status = p.Status,
        //        Amount = p.Amount,
        //        UserId = p.UserId,
        //        CreatedAt = p.CreatedAt
        //    }));
        //}

        //[HttpGet]
        //[Route("Debug/UpdateStatus")]
        //public async Task<IActionResult> TestUpdateStatusGet(string paymentId, string status)
        //{
        //    var result = await _paymentService.UpdateStatusAsync(paymentId, status);
        //    return Json(new { success = result, paymentId, status });
        //}

        //[HttpPost]
        //[Route("Debug/UpdateStatus")]
        //public async Task<IActionResult> TestUpdateStatus(string paymentId, string status)
        //{
        //    var result = await _paymentService.UpdateStatusAsync(paymentId, status);
        //    return Json(new { success = result, paymentId, status });
        //}

        //[HttpGet("/Debug/DecodeMomo")]
        //public IActionResult DecodeMomo(string t = "")
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(t))
        //        {
        //            return Json(new { error = "No parameter provided" });
        //        }

        //        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(t));
        //        return Json(new { original = t, decoded = decoded });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { error = ex.Message, parameter = t });
        //    }
        //}
    }


}