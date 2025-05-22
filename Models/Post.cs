using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
namespace Vintellitour_Framework.Models
{
    [BsonIgnoreExtraElements]
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("author_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AuthorId { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("locationRaw")]
        public string LocationRaw { get; set; }

        [BsonElement("media")]
        public List<Media> Media { get; set; }

        [BsonElement("timestamp")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; }

        [BsonElement("likes")]
        [BsonRepresentation(BsonType.Int64)]
        public int Likes { get; set; }

        [BsonElement("comments")]
        public List<Comment> Comments { get; set; }

        [BsonElement("usersLiked")]
        public List<string> UsersLiked { get; set; }

        [BsonElement("flagInfo")]
        public FlagInfo FlagInfo { get; set; }

        [BsonElement("provinceGid")]
        public int ProvinceGid { get; set; }  
    }

    public class Media
    {
        [BsonElement("media_type")]
        public string MediaType { get; set; }

        [BsonElement("media_url")]
        public string MediaUrl { get; set; }
    }

    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }

    public class FlagInfo
    {
        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("word")]
        public string Word { get; set; }

        [BsonElement("source")]
        public string Source { get; set; }

        [BsonElement("flaggedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime FlaggedAt { get; set; }
    }

    public class PostViewModel
    {
        public Post Post { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAvatar { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }

}