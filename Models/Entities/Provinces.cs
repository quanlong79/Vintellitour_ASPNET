using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vintellitour_Framework.Models.Entities
{
    public class Province
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        public int Gid { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;

        // Thêm các trường khác nếu cần
    }
}
