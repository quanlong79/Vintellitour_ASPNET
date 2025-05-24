using MongoDB.Driver;
using Vintellitour_Framework.Models;
using Microsoft.Extensions.Options;

namespace Vintellitour_Framework.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<Provinces> _provinces;
        private readonly IMongoCollection<LocationsModel> _locations;

        public MongoDbService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            if (mongoDbSettings?.Value?.ConnectionString == null || mongoDbSettings?.Value?.DatabaseName == null)
            {
                throw new ArgumentNullException("MongoDbSettings", "ConnectionString or DatabaseName is null in appsettings.json.");
            }

            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _users = database.GetCollection<User>("users");
            _posts = database.GetCollection<Post>("posts");
            _provinces = database.GetCollection<Provinces>("provinces");
            _locations = database.GetCollection<LocationsModel>("locations");
        }

        // Phương thức trả về collection Users
        public IMongoCollection<User> GetUserCollection()
        {
            return _users;
        }
        public IMongoCollection<Post> GetPostCollection()
        {
            return _posts;
        }
        public IMongoCollection<Provinces> GetProvincesCollection()
        {
            return _provinces;
        }
        public IMongoCollection<LocationsModel> GetLocationsCollection()
        {
            return _locations;
        }

        //Lấy tất cả người dùng
        public async Task<List<User>> GetUsersAsync() =>
            await _users.Find(user => true).ToListAsync();

        // Lấy người dùng theo email
        public async Task<User> GetUserByEmailAsync(string email) =>
            await _users.Find(user => user.Email == email).FirstOrDefaultAsync();

        // Tạo người dùng mới
        public async Task CreateUserAsync(User user) =>
            await _users.InsertOneAsync(user);

        // Cập nhật người dùng
        public async Task UpdateUserAsync(string id, User user) =>
            await _users.ReplaceOneAsync(u => u.Id == id, user);

        // Xóa người dùng
        public async Task DeleteUserAsync(string id) =>
            await _users.DeleteOneAsync(user => user.Id == id);
     }
}
