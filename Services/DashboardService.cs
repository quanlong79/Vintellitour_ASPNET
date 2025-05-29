using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.ViewModels;

namespace Vintellitour_Framework.Services
{
    public class DashboardService
    {
        private readonly IMongoCollection<Post> _postsCollection;
        private readonly IMongoCollection<LocationsModel> _locationsCollection;
        private readonly IMongoCollection<Provinces> _provincesCollection;
        private readonly IMongoCollection<User> _usersCollection;

        public DashboardService(IMongoDatabase database)
        {
            _postsCollection = database.GetCollection<Post>("posts");
            _locationsCollection = database.GetCollection<LocationsModel>("locations");
            _provincesCollection = database.GetCollection<Provinces>("provinces");
            _usersCollection = database.GetCollection<User>("users");
        }

        public async Task<List<ProvinceEngagementViewModel>> GetTop5ProvincesByEngagementAsync()
        {
            var posts = await _postsCollection.Find(FilterDefinition<Post>.Empty).ToListAsync();
            var locations = await _locationsCollection.Find(FilterDefinition<LocationsModel>.Empty).ToListAsync();
            var provinces = await _provincesCollection.Find(FilterDefinition<Provinces>.Empty).ToListAsync();

            // Tính tổng tương tác cho mỗi post: likes + comments count
            var postEngagements = posts.Select(p => new
            {
                p.Id,
                ProvinceGid = p.ProvinceGid, // trong Post bạn đã có ProvinceGid rồi, không cần join Location
                TotalEngagement = p.Likes + (p.Comments?.Count ?? 0)
            });

            // Group theo ProvinceGid và tính tổng engagement
            var engagementByProvince = postEngagements
                .GroupBy(x => x.ProvinceGid)
                .Select(g => new
                {
                    ProvinceGid = g.Key,
                    TotalEngagement = g.Sum(x => x.TotalEngagement)
                });

            // Join với provinces để lấy tên tỉnh
            var result = (from e in engagementByProvince
                          join prov in provinces on e.ProvinceGid equals prov.Id
                          orderby e.TotalEngagement descending
                          select new ProvinceEngagementViewModel
                          {
                              ProvinceGid = e.ProvinceGid,
                              ProvinceName = prov.Name,
                              TotalEngagement = e.TotalEngagement
                          }).Take(5).ToList();

            return result;
        }

        public async Task<UserPostStatusViewModel> GetUserPostStatusAsync()
        {
            var users = await _usersCollection.Find(FilterDefinition<User>.Empty).ToListAsync();
            var posts = await _postsCollection.Find(FilterDefinition<Post>.Empty).ToListAsync();

            var usersWithPosts = posts
                .Select(p => p.AuthorId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .Count();

            var totalUsers = users.Count;
            var usersWithoutPosts = totalUsers - usersWithPosts;
            if (usersWithoutPosts < 0) usersWithoutPosts = 0;

            return new UserPostStatusViewModel
            {
                UsersWithPosts = usersWithPosts,
                UsersWithoutPosts = usersWithoutPosts
            };
        }

        public async Task<List<PostStatsByMonthViewModel>> GetPostStatsByMonthAsync(int year)
        {
            var filter = Builders<Post>.Filter.And(
                Builders<Post>.Filter.Gte(p => p.Timestamp, new System.DateTime(year, 1, 1)),
                Builders<Post>.Filter.Lt(p => p.Timestamp, new System.DateTime(year + 1, 1, 1))
            );

            var pipeline = _postsCollection.Aggregate()
                .Match(filter)
                .Group(new MongoDB.Bson.BsonDocument
                {
                    { "_id", new MongoDB.Bson.BsonDocument { { "month", new MongoDB.Bson.BsonDocument("$month", "$timestamp") } } },
                    { "count", new MongoDB.Bson.BsonDocument("$sum", 1) }
                })
                .Project(new MongoDB.Bson.BsonDocument
                {
                    { "Month", "$_id.month" },
                    { "PostCount", "$count" },
                    { "_id", 0 }
                })
                .Sort(new MongoDB.Bson.BsonDocument("Month", 1));

            var result = await pipeline.ToListAsync();

            return result.Select(r => new PostStatsByMonthViewModel
            {
                Month = r.GetValue("Month").AsInt32,
                PostCount = r.GetValue("PostCount").AsInt32
            }).ToList();
        }
    }
}
