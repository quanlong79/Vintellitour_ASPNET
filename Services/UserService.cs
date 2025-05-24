using Vintellitour_Framework.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Vintellitour_Framework.Services
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(string username, string email, string password);
        Task<User> LoginUserAsync(string email, string password);
        Task<List<User>> GetUsersByIdsAsync(List<string> userIds);
        Task<User> GetUserIdAsync(string userId);
        Task UpdateUserAsync(User user);
    }

    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(MongoDbService mongoDbService)
        {
            _users = mongoDbService.GetUserCollection();
        }

        public async Task<List<User>> GetUsersByIdsAsync(List<string> userIds)
        {
            var filter = Builders<User>.Filter.In(u => u.Id, userIds);
            return await _users.Find(filter).ToListAsync();
        }

        public async Task<User> GetUserIdAsync(string userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User> RegisterUserAsync(string username, string email, string password)
        {
            var existingUser = await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return null; // Email đã tồn tại
            }

            // Hash mật khẩu
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Email = email,
                Password = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _users.InsertOneAsync(newUser);
            return newUser;
        }

        public async Task<User> LoginUserAsync(string email, string password)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null) return null;

            bool verified = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!verified) return null;

            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Id))
                throw new ArgumentException("User hoặc User.Id không hợp lệ");

            user.UpdatedAt = DateTime.UtcNow;

            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<User>.Update
                .Set(u => u.Username, user.Username)
                .Set(u => u.Avatar, user.Avatar)
                .Set(u => u.UpdatedAt, user.UpdatedAt);

            await _users.UpdateOneAsync(filter, update);
        }
    }
}
