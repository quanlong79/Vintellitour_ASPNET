using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Vintellitour_Framework.Models
{
    [BsonIgnoreExtraElements] // Bỏ qua các trường không khai báo trong class
    public class AdminUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }  // _id

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("avatar")]
        public string Avatar { get; set; }

        [BsonElement("isVerified")]
        public bool IsVerified { get; set; }

        [BsonElement("VerificationToken")]
        [BsonIgnoreIfNull]
        public string VerificationToken { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
