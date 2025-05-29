using System.Threading.Tasks;
using Vintellitour_Framework.Models.DTOs;

namespace Vintellitour_Framework.Services.Interfaces
{
    public interface IMapService
    {
        Task<ProvinceDto> GetProvinceByGid(int gid);
        // Các phương thức khác...
    }
}
