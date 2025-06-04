using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Vintellitour_Framework.Models
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("amount")]
        public decimal Amount { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } // Pending, Success, Cancel

        [BsonElement("shippingStatus")]
        public string ShippingStatus { get; set; } = null; // Có thể null hoặc giá trị mặc định

        [BsonElement("noteOrder")]
        public string NoteOrder { get; set; }  // <--- Thêm thuộc tính này

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("details")]
        public List<DetailPayment> Details { get; set; }
    }

}
