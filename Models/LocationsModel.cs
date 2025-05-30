using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;
using Vintellitour_Framework.Models.Entities;

namespace Vintellitour_Framework.Models
{
    public class Coordinates
    {
        [BsonElement("lat")]
        public double Lat { get; set; }

        [BsonElement("lng")]
        public double Lng { get; set; }
    }

    public class LocationsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("coordinates")]
        public Coordinates Coordinates { get; set; }

        [BsonElement("provinceGid")]
        [BsonRepresentation(BsonType.Int32)]
        public int ProvinceGid { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("description_history")]
        public string DescriptionHistory { get; set; }

        [BsonElement("image")]
        public List<string> Image { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("openTime")]
        public string OpenTime { get; set; }

        [BsonElement("price")]
        public string Price { get; set; }

        [BsonElement("streetViewUrls")]
        public List<string> StreetViewUrls { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; }
    }
    public class LocationResponse
    {
        public bool Success { get; set; }
        public List<Location> Data { get; set; } = new List<Location>();
        public string Message { get; set; } = string.Empty;
    }

    public class ProvinceFeature
    {
        public string Type { get; set; } = "Feature";
        public ProvinceProperties Properties { get; set; } = new ProvinceProperties();
        public object Geometry { get; set; } = new object();
    }

    public class ProvinceProperties
    {
        public string TenTinh { get; set; } = string.Empty;
        public int Gid { get; set; }
    }

    public class MapViewModel
    {
        public string ProvinceGeoJson { get; set; } = string.Empty;
        public int? SelectedProvinceGid { get; set; }
        public string? SelectedProvinceName { get; set; }
    }
}
