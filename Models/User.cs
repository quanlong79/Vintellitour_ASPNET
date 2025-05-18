using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Vintellitour_Framework.Models
{
    [BsonIgnoreExtraElements]  // Bỏ qua các trường extra không khai báo
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("avatar")]
        public string Avatar { get; set; } = "";

        [BsonElement("isVerified")]
        public bool IsVerified { get; set; } = true;

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; }
    }
}
