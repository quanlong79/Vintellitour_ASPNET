using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IMongoCollection<AdminUser> _collection;

        public AdminUserService(IMongoDatabase database)
        {
            _collection = database.GetCollection<AdminUser>("users");
        }

        public async Task<List<AdminUser>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            var result = await _collection.DeleteOneAsync(u => u.Id == userId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> UpdateAsync(string id, string username, string email)
        {
            var updateDef = Builders<AdminUser>.Update
                .Set(u => u.Username, username)
                .Set(u => u.Email, email)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(u => u.Id == id, updateDef);
            return result.ModifiedCount > 0;
        }
    }
}
