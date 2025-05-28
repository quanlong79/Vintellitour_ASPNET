using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Data;
using Vintellitour_Framework.Data.Repositories;
using Vintellitour_Framework.Models.Entities;

namespace YourNamespace.Data.Repositories
{
    public class ProvinceRepository : IProvinceRepository
    {
        private readonly IMongoCollection<Province> _provinces;

        public ProvinceRepository(MongoDbContext context)
        {
            _provinces = context.Provinces;
        }

        public async Task<Province?> GetByGidAsync(int gid)
        {
            return await _provinces.Find(p => p.Gid == gid).FirstOrDefaultAsync();
        }

        public async Task<List<Province>> SearchByNameAsync(string nameQuery)
        {
            return await _provinces.Find(p => p.Name.ToLower().Contains(nameQuery.ToLower())).ToListAsync();
        }

        public async Task<List<Province>> GetAllAsync()
        {
            return await _provinces.Find(_ => true).ToListAsync();
        }
    }
}
