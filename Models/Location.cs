using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vintellitour_Framework.Models
{
    [BsonIgnoreExtraElements]
    public class Location
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("coordinates")]
        public Coordinates Coordinates { get; set; }

        [BsonElement("provinceGid")]
        public int ProvinceGid { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("description_history")]
        public string DescriptionHistory { get; set; }

        [BsonElement("image")]
        public string[] Image { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("openTime")]
        public string OpenTime { get; set; }

        [BsonElement("price")]
        public string Price { get; set; }

        [BsonElement("streetViewUrls")]
        public string[] StreetViewUrls { get; set; }

        [BsonElement("tags")]
        public string[] Tags { get; set; }
    }
}
