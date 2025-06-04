using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Vintellitour_Framework.Models.Entities
{
    [BsonIgnoreExtraElements] // Bỏ qua trường không có trong class
    public class Location
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]  // Chuyển đổi ObjectId sang string
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]  // Map chính xác tên trường trong document
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("slug")]
        public string Slug { get; set; } = string.Empty;

        [BsonElement("coordinates")]
        public Coordinates Coordinates { get; set; } = new Coordinates();

        [BsonElement("provinceGid")]
        public int ProvinceGid { get; set; }

        // Thêm các trường có trong dữ liệu MongoDB bạn gửi
        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("description_history")]
        public string? DescriptionHistory { get; set; }

        [BsonElement("image")]
        public List<string>? Image { get; set; }

        [BsonElement("openTime")]
        public string? OpenTime { get; set; }

        [BsonElement("price")]
        public string? Price { get; set; }

        [BsonElement("streetViewUrls")]
        public List<string> StreetViewUrls { get; set; } = new List<string>();

        [BsonElement("tags")]
        public List<string>? Tags { get; set; }
    }

    public class Coordinates
    {
        [BsonElement("lat")]
        public double Lat { get; set; }

        [BsonElement("lng")]
        public double Lng { get; set; }
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
