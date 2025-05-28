using MongoDB.Bson.Serialization.Attributes;

namespace Vintellitour_Framework.Models
{
    public class CartItem
    {
        [BsonElement("productId")]
        public string ProductId { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}
