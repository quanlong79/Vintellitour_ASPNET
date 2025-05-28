using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Models.DTOs;
namespace Vintellitour_Framework.Services.Interfaces
{
    public interface ILocationService
    {
        Task<LocationDto?> GetLocationByIdAsync(string id);

        Task<List<LocationDto>> GetLocationsByProvinceGid(int provinceGid);

        // Có thể thêm các method khác như tạo, sửa, xóa bài viết location nếu cần
    }
}
