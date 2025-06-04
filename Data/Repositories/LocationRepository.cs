using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Data;
using Vintellitour_Framework.Data.Repositories;
using Vintellitour_Framework.Models.Entities;
using MongoDB.Bson;
namespace Vintellitour_Framework    .Data.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly IMongoCollection<Location> _locations;

        public LocationRepository(MongoDbContext context)
        {
            _locations = context.Locations;
        }

        public async Task<Location?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                return null; // hoặc throw exception, tùy bạn xử lý
            }

            return await _locations.Find(l => l.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Location>> GetByProvinceGidAsync(int provinceGid)
        {
            return await _locations.Find(l => l.ProvinceGid == provinceGid).ToListAsync();
        }
        public async Task<Location> AddAsync(Location location)
        {
            // Nếu bạn dùng MongoDB
            await _locations.InsertOneAsync(location);
            return location;
        }
        public async Task UpdateAsync(Location location)
        {
            var filter = Builders<Location>.Filter.Eq(l => l.Id, location.Id);
            await _locations.ReplaceOneAsync(filter, location);
        }
        public async Task<List<Location>> GetAllAsync()
        {
            return await _locations.Find(_ => true).ToListAsync();
        }
        public async Task DeleteAsync(Location location)
        {
            // Use _locations for MongoDB, not _context
            var filter = Builders<Location>.Filter.Eq(l => l.Id, location.Id);
            await _locations.DeleteOneAsync(filter);  // Use DeleteOneAsync for MongoDB
        }

    }
}
