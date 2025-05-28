using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vintellitour_Framework.Services.Interfaces;
using Vintellitour_Framework.Data.Repositories;
using Vintellitour_Framework.Models.DTOs;
namespace Vintellitour_Framework.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<LocationDto?> GetLocationByIdAsync(string id)
        {
            var entity = await _locationRepository.GetByIdAsync(id);
            if (entity == null)
                return null;

            return MapToDto(entity);
        }

        public async Task<List<LocationDto>> GetLocationsByProvinceGid(int provinceGid)
        {
            var entities = await _locationRepository.GetByProvinceGidAsync(provinceGid);
            return entities.Select(MapToDto).ToList();
        }

        private LocationDto MapToDto(Models.Entities.Location entity)
        {
            return new LocationDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Address = entity.Address,
                Coordinates = new CoordinatesDto
                {
                    Lat = entity.Coordinates.Lat,   // Phải truy cập đúng object Coordinates
                    Lng = entity.Coordinates.Lng
                }
            };
        }
    }
}
