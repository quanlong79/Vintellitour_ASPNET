using MongoDB.Bson;
using MongoDB.Driver;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Services
{
    public class PaymentService
    {
        private readonly IMongoCollection<Payment> _payments;

        public PaymentService(IMongoDatabase db)
        {
            _payments = db.GetCollection<Payment>("payments");
        }

        public async Task<Payment> CreatePaymentAsync(string userId, decimal amount, string status, List<DetailPayment> details)
        {
            var payment = new Payment
            {
                UserId = userId,
                Amount = amount,
                Status = status,
                Details = details
            };

            await _payments.InsertOneAsync(payment);
            return payment;
        }

        public async Task<bool> UpdateStatusAsync(string paymentId, string status)
        {
            if (!ObjectId.TryParse(paymentId, out ObjectId objId))
            {
                // Id không hợp lệ, có thể trả về false hoặc throw exception
                return false;
            }

            var filter = Builders<Payment>.Filter.Eq(p => p.Id, objId);
            var update = Builders<Payment>.Update
                .Set(p => p.Status, status)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _payments.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<Payment?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return null; // Hoặc throw lỗi vì id không hợp lệ
            }

            return await _payments.Find(p => p.Id == objectId).FirstOrDefaultAsync();
        }
    }
}
