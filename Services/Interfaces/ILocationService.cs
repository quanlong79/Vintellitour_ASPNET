using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Models.DTOs;
using Vintellitour_Framework.Models.Entities;

namespace Vintellitour_Framework.Services.Interfaces
{
    public interface ILocationService
    {
        IMongoCollection<Location> Locations { get; }
        // Lấy địa điểm theo ID
        Task<LocationDto?> GetLocationByIdAsync(string id);

        // Lấy danh sách địa điểm theo ProvinceGid
        Task<List<LocationDto>> GetLocationsByProvinceGid(int provinceGid);

        // Thêm địa điểm mới, nhận vào DTO và trả về DTO sau khi thêm
        Task<LocationDto> AddLocationAsync(LocationDto locationDto);

        // Cập nhật địa điểm, nhận vào DTO và cập nhật trong DB
        Task UpdateLocationAsync(LocationDto locationDto);

        // Các phương thức khác có thể thêm vào nếu cần (xóa, tìm kiếm, v.v.)
        Task DeleteLocationAsync(string id);

        Task<List<LocationDto>> GetAllLocationsAsync();

    }
}
