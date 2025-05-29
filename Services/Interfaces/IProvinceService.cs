using Vintellitour_Framework.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Vintellitour_Framework.Services.Interfaces
{
    public interface IProvinceService
    {
        Task<ProvinceDto?> GetProvinceByGid(int gid);
        Task<List<string>> SearchProvinces(string query);
    }
}
