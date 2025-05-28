using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Models.Entities;
namespace Vintellitour_Framework.Data.Repositories
{
    public interface IProvinceRepository
    {
        Task<Province?> GetByGidAsync(int gid);
        Task<List<Province>> SearchByNameAsync(string nameQuery);
        Task<List<Province>> GetAllAsync();
    }
}
