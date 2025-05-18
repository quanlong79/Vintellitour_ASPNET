using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Vintellitour_Framework.Models
{
    public class Provinces
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int32)] // vì _id là số nguyên 58
        public int Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("cuisine")]
        public CuisineInfo Cuisine { get; set; }

        [BsonElement("culture")]
        public CultureInfo Culture { get; set; }
    }

    public class CuisineInfo
    {
        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }
    }

    public class CultureInfo
    {
        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }
    }

}
