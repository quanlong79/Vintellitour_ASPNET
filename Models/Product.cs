using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Vintellitour_Framework.Models
{
    [BsonIgnoreExtraElements]
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("image")]
        public string Image { get; set; } = "/img/placeholder.png";

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("originalPrice")]
        public decimal OriginalPrice { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("rating")]
        public double Rating { get; set; } = 0;

        [BsonElement("reviews")]
        public int Reviews { get; set; } = 0;

        [BsonElement("isNew")]
        public bool IsNew { get; set; } = false;

        [BsonElement("isBestSeller")]
        public bool IsBestSeller { get; set; } = false;

        [BsonElement("stock")]
        public int Stock { get; set; } = 0; // 👈 Thêm dòng này để quản lý tồn kho

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
