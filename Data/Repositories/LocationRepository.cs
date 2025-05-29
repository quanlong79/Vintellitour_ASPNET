using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Data;
using Vintellitour_Framework.Data.Repositories;
using Vintellitour_Framework.Models.Entities;

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
            return await _locations.Find(l => l.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Location>> GetByProvinceGidAsync(int provinceGid)
        {
            return await _locations.Find(l => l.ProvinceGid == provinceGid).ToListAsync();
        }

        public async Task<List<Location>> GetAllAsync()
        {
            return await _locations.Find(_ => true).ToListAsync();
        }
    }
}
