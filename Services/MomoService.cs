using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Vintellitour_Framework.Models;
using RestSharp;
using Newtonsoft.Json;

namespace Vintellitour_Framework.Services
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;

        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
        {
            try
            {
                Console.WriteLine($"=== Creating MoMo Payment ===");
                Console.WriteLine($"OrderId: {model.OrderId}");
                Console.WriteLine($"Amount: {model.Amount}");
                Console.WriteLine($"FullName: {model.FullName}");

                model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;

                // Use the NEW API format
                var requestId = model.OrderId;
                var orderId = model.OrderId;
                var orderInfo = model.OrderInfo;
                var amount = model.Amount.ToString();
                var extraData = ""; // Can be empty

                Console.WriteLine($"Using URLs - Return: {_options.Value.ReturnUrl}, Notify: {_options.Value.NotifyUrl}");

                // NEW API signature format
                var rawSignature = $"accessKey={_options.Value.AccessKey}" +
                                 $"&amount={amount}" +
                                 $"&extraData={extraData}" +
                                 $"&ipnUrl={_options.Value.NotifyUrl}" +
                                 $"&orderId={orderId}" +
                                 $"&orderInfo={orderInfo}" +
                                 $"&partnerCode={_options.Value.PartnerCode}" +
                                 $"&redirectUrl={_options.Value.ReturnUrl}" +
                                 $"&requestId={requestId}" +
                                 $"&requestType=captureWallet"; // Updated request type

                Console.WriteLine($"Raw signature: {rawSignature}");

                var signature = ComputeHmacSha256(rawSignature, _options.Value.SecretKey);
                Console.WriteLine($"Computed signature: {signature}");

                // Use the NEW API endpoint
                var client = new RestClient("https://test-payment.momo.vn/v2/gateway/api/create");
                var request = new RestRequest() { Method = Method.Post };
                request.AddHeader("Content-Type", "application/json; charset=UTF-8");

                // NEW API request format
                var requestData = new
                {
                    partnerCode = _options.Value.PartnerCode,
                    partnerName = "Test",
                    storeId = _options.Value.PartnerCode,
                    requestId = requestId,
                    amount = long.Parse(amount),
                    orderId = orderId,
                    orderInfo = orderInfo,
                    redirectUrl = _options.Value.ReturnUrl,
                    ipnUrl = _options.Value.NotifyUrl,
                    lang = "vi",
                    extraData = extraData,
                    requestType = "captureWallet", // Updated request type
                    signature = signature
                };

                var jsonRequest = JsonConvert.SerializeObject(requestData);
                Console.WriteLine($"Request JSON: {jsonRequest}");

                request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);

                var response = await client.ExecuteAsync(request);
                Console.WriteLine($"Response status: {response.StatusCode}");
                Console.WriteLine($"Response content: {response.Content}");

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"HTTP Error: {response.StatusCode} - {response.ErrorMessage}");
                    return new MomoCreatePaymentResponseModel
                    {
                        ResultCode = -1,
                        Message = $"HTTP Error: {response.StatusCode}",
                        PayUrl = null
                    };
                }

                var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);

                if (momoResponse != null)
                {
                    Console.WriteLine($"MoMo Response - ResultCode: {momoResponse.ResultCode}");
                    Console.WriteLine($"MoMo Response - Message: {momoResponse.Message}");
                    Console.WriteLine($"MoMo Response - PayUrl: {momoResponse.PayUrl}");
                }

                return momoResponse ?? new MomoCreatePaymentResponseModel
                {
                    ResultCode = -1,
                    Message = "Failed to parse response",
                    PayUrl = null
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreatePaymentAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new MomoCreatePaymentResponseModel
                {
                    ResultCode = -1,
                    Message = ex.Message,
                    PayUrl = null
                };
            }
        }

        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.First(s => s.Key == "amount").Value;
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            var orderId = collection.First(s => s.Key == "orderId").Value;

            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo
            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hashString;
        }
    }
}