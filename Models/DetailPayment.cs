using MongoDB.Bson.Serialization.Attributes;

namespace Vintellitour_Framework.Models
{
    public class DetailPayment
    {
        [BsonElement("productId")]
        public string ProductId { get; set; }

        [BsonElement("productName")]
        public string ProductName { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonIgnore]
        public decimal TotalPrice => Price * Quantity;
    }
}


