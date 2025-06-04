using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Models.Entities;

namespace Vintellitour_Framework.Data.Repositories
{
    public interface ILocationRepository
    {
        Task<Location?> GetByIdAsync(string id);
        Task<List<Location>> GetByProvinceGidAsync(int provinceGid);
        Task<List<Location>> GetAllAsync();
        Task<Location> AddAsync(Location location);
        Task UpdateAsync(Location location);
        Task DeleteAsync(Location location);

    }
}