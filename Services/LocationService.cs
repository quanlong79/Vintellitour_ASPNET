using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vintellitour_Framework.Data;
using Vintellitour_Framework.Data.Repositories;
using Vintellitour_Framework.Models.DTOs;
using Vintellitour_Framework.Models.Entities;
using Vintellitour_Framework.Services.Interfaces;
using static Vintellitour_Framework.Models.DTOs.LocationDto;

namespace Vintellitour_Framework.Services
{
    public class LocationService : ILocationService
    {
        private readonly MongoDbContext _context;
        private readonly ILocationRepository _locationRepository;

        // Constructor để inject repository
        public LocationService(MongoDbContext context, ILocationRepository locationRepository)
        {
            _context = context;
            _locationRepository = locationRepository;
        }

        public IMongoCollection<Location> Locations => _context.Locations;
        // Lấy địa điểm theo ID
        public async Task<List<LocationDto>> GetAllLocationsAsync()
        {
            var entities = await _locationRepository.GetAllAsync();  // Fetch all locations from repository
            return entities.Select(MapToDto).ToList();  // Map the entities to DTOs and return
        }
        public async Task<LocationDto?> GetLocationByIdAsync(string id)
        {
            var entity = await _locationRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return null;
            }

            return MapToDto(entity);  // Chuyển đổi entity thành DTO
        }

        public async Task DeleteLocationAsync(string id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
            {
                throw new Exception("Location not found");
            }

            await _locationRepository.DeleteAsync(location); // Now works with DeleteAsync
        }
        // Lấy danh sách địa điểm theo ProvinceGid
        public async Task<List<LocationDto>> GetLocationsByProvinceGid(int provinceGid)
        {
            var entities = await _locationRepository.GetByProvinceGidAsync(provinceGid);
            return entities.Select(MapToDto).ToList();  // Chuyển đổi danh sách entity thành danh sách DTO
        }

        // Thêm địa điểm mới
        public async Task<LocationDto> AddLocationAsync(LocationDto locationDto)
        {
            var location = MapToEntity(locationDto);  // Chuyển DTO thành entity
            var addedLocation = await _locationRepository.AddAsync(location);  // Thêm vào cơ sở dữ liệu
            return MapToDto(addedLocation);  // Trả về đối tượng DTO sau khi thêm
        }

        // Cập nhật thông tin địa điểm
        public async Task UpdateLocationAsync(LocationDto locationDto)
        {
            var location = MapToEntity(locationDto);  // Chuyển DTO thành entity
            await _locationRepository.UpdateAsync(location);  // Cập nhật vào cơ sở dữ liệu
        }

        // Chuyển đổi từ entity thành DTO
        private LocationDto MapToDto(Location entity)
        {
            return new LocationDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Address = entity.Address,
                Coordinates = new CoordinatesDto
                {
                    Lat = entity.Coordinates.Lat,
                    Lng = entity.Coordinates.Lng
                },
                DescriptionHistory = entity.DescriptionHistory, // Thêm trường DescriptionHistory vào DTO
                Slug = entity.Slug,
                ProvinceGid = entity.ProvinceGid,
                OpenTime = entity.OpenTime,
                Price = entity.Price,
                StreetViewUrls = entity.StreetViewUrls,
                Tags = entity.Tags,
                Image = entity.Image
            };
        }

        // Chuyển đổi từ DTO thành entity
        private Location MapToEntity(LocationDto locationDto)
        {
            return new Location
            {
                Id = locationDto.Id,
                Name = locationDto.Name ?? string.Empty,
                Description = locationDto.Description,
                Address = locationDto.Address ?? string.Empty,
                DescriptionHistory = locationDto.DescriptionHistory,  // Thêm trường DescriptionHistory vào entity
                Slug = locationDto.Slug ?? string.Empty,
                ProvinceGid = locationDto.ProvinceGid,
                OpenTime = locationDto.OpenTime,
                Price = locationDto.Price,
                StreetViewUrls = locationDto.StreetViewUrls ?? new List<string>(),
                Tags = locationDto.Tags ?? new List<string>(),
                Image = locationDto.Image ?? new List<string>(),
                Coordinates = new Coordinates
                {
                    Lat = locationDto.Coordinates?.Lat ?? 0,  // Nếu null, dùng mặc định 0
                    Lng = locationDto.Coordinates?.Lng ?? 0  // Nếu null, dùng mặc định 0
                }
            };
        }
    }
}
