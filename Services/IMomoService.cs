using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
