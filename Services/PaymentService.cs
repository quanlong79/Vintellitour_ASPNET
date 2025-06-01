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
                Details = details,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await _payments.InsertOneAsync(payment);
            
            Console.WriteLine($"Created payment with ID: {payment.Id} (ObjectId: {payment.Id.ToString()})");
            
            return payment;
        }

        public async Task<bool> UpdateStatusAsync(string paymentId, string status, string noteOrder = null)
        {
            Console.WriteLine($"Attempting to update payment status - PaymentId: '{paymentId}', NewStatus: '{status}'");

            if (string.IsNullOrWhiteSpace(paymentId))
            {
                Console.WriteLine("PaymentId is null or empty");
                return false;
            }

            if (!ObjectId.TryParse(paymentId, out ObjectId objId))
            {
                Console.WriteLine($"Failed to parse PaymentId '{paymentId}' as ObjectId");

                var stringFilter = Builders<Payment>.Filter.Eq("_id", paymentId);

                var stringUpdate = Builders<Payment>.Update
                    .Set(p => p.Status, status)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                if (!string.IsNullOrEmpty(noteOrder))
                {
                    stringUpdate = stringUpdate.Set(p => p.NoteOrder, noteOrder);
                }

                var stringResult = await _payments.UpdateOneAsync(stringFilter, stringUpdate);

                Console.WriteLine($"String ID update result - MatchedCount: {stringResult.MatchedCount}, ModifiedCount: {stringResult.ModifiedCount}");

                if (stringResult.ModifiedCount > 0)
                {
                    Console.WriteLine($"Successfully updated payment status using string ID");
                    return true;
                }

                var allPayments = await _payments.Find(_ => true).ToListAsync();
                Console.WriteLine($"Total payments in database: {allPayments.Count}");
                foreach (var p in allPayments)
                {
                    Console.WriteLine($"Payment ID in DB: {p.Id}, String representation: {p.Id.ToString()}");
                }

                return false;
            }

            Console.WriteLine($"Successfully parsed ObjectId: {objId}");

            var existingPayment = await _payments.Find(p => p.Id == objId).FirstOrDefaultAsync();
            if (existingPayment == null)
            {
                Console.WriteLine($"Payment with ObjectId {objId} not found in database");
                return false;
            }

            Console.WriteLine($"Found payment - Current Status: '{existingPayment.Status}', Amount: {existingPayment.Amount}");

            var filter = Builders<Payment>.Filter.Eq(p => p.Id, objId);
            var update = Builders<Payment>.Update
                .Set(p => p.Status, status)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(noteOrder))
            {
                update = update.Set(p => p.NoteOrder, noteOrder);
            }

            var result = await _payments.UpdateOneAsync(filter, update);

            Console.WriteLine($"Update result - MatchedCount: {result.MatchedCount}, ModifiedCount: {result.ModifiedCount}");

            if (result.ModifiedCount > 0)
            {
                Console.WriteLine($"Successfully updated payment {paymentId} status to '{status}'");

                var updatedPayment = await _payments.Find(p => p.Id == objId).FirstOrDefaultAsync();
                Console.WriteLine($"Verified - New status: {updatedPayment?.Status}");

                return true;
            }
            else
            {
                Console.WriteLine($"Failed to update payment {paymentId} - No documents modified");
                return false;
            }
        }

        public async Task<Payment?> GetByIdAsync(string id)
        {
            Console.WriteLine($"Getting payment by ID: '{id}'");

            if (string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine("ID is null or empty");
                return null;
            }

            // Thử ObjectId trước
            if (ObjectId.TryParse(id, out ObjectId objectId))
            {
                Console.WriteLine($"Using ObjectId: {objectId}");
                var payment = await _payments.Find(p => p.Id == objectId).FirstOrDefaultAsync();
                
                if (payment != null)
                {
                    Console.WriteLine($"Found payment with ObjectId - Status: {payment.Status}");
                }
                else
                {
                    Console.WriteLine($"No payment found with ObjectId: {objectId}");
                }
                
                return payment;
            }

            // Nếu không parse được ObjectId, thử tìm theo string
            Console.WriteLine($"ObjectId parse failed, trying string search for: '{id}'");
            return await _payments.Find(Builders<Payment>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }

        // Thêm method để debug
        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _payments.Find(_ => true).ToListAsync();
        }

        public async Task<List<Payment>> GetByUserIdAsync(string userId)
        {
            return await _payments.Find(p => p.UserId == userId)
                                  .SortByDescending(p => p.CreatedAt)
                                  .ToListAsync();
        }

        public async Task<bool> UpdateShippingStatusAsync(string paymentId, string shippingStatus)
        {
            if (!ObjectId.TryParse(paymentId, out ObjectId objId))
                return false;

            var filter = Builders<Payment>.Filter.Eq(p => p.Id, objId);
            var update = Builders<Payment>.Update
                .Set(p => p.ShippingStatus, shippingStatus)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _payments.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }


    }
}