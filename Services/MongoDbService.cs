using MongoDB.Driver;
using Vintellitour_Framework.Models;
using Microsoft.Extensions.Options;

namespace Vintellitour_Framework.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<User> _users;

        public MongoDbService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            if (mongoDbSettings?.Value?.ConnectionString == null || mongoDbSettings?.Value?.DatabaseName == null)
            {
                throw new ArgumentNullException("MongoDbSettings", "ConnectionString or DatabaseName is null in appsettings.json.");
            }

            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        // Phương thức trả về collection Users
        public IMongoCollection<User> GetUserCollection()
        {
            return _users;
        }

        // Lấy tất cả người dùng
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
