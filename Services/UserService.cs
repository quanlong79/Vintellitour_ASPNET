using Vintellitour_Framework.Models;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Vintellitour_Framework.Services
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(string username, string email, string password);
        Task<User> LoginUserAsync(string email, string password);
        Task<List<User>> GetUsersByIdsAsync(List<string> userIds);
    }

    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(MongoDbService mongoDbService)
        {
            _users = mongoDbService.GetUserCollection();  // Lấy collection Users từ MongoDbService
        }
        public async Task<List<User>> GetUsersByIdsAsync(List<string> userIds)
        {
            var filter = Builders<User>.Filter.In(u => u.Id, userIds);
            return await _users.Find(filter).ToListAsync();
        }
        public async Task<User> RegisterUserAsync(string username, string email, string password)
        {
            var existingUser = await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return null;  // Người dùng đã tồn tại
            }

            var newUser = new User
            {
                Username = username,
                Email = email,
                Password = password,  // Mã hóa mật khẩu trước khi lưu (nếu cần)
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _users.InsertOneAsync(newUser);
            return newUser;
        }

        public async Task<User> LoginUserAsync(string email, string password)
        {
            var user = await _users.Find(u => u.Email == email && u.Password == password).FirstOrDefaultAsync();
            return user;
        }
    }
}
