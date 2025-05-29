
using Vintellitour_Framework.Services.Interfaces;
using Vintellitour_Framework.Data.Repositories;
using Vintellitour_Framework.Models.DTOs;
namespace Vintellitour_Framework.Services
{
    public class MapService : IMapService
    {
        private readonly IProvinceRepository _provinceRepository;

        public MapService(IProvinceRepository provinceRepository)
        {
            _provinceRepository = provinceRepository;
        }

        public async Task<ProvinceDto> GetProvinceByGid(int gid)
        {
            var province = await _provinceRepository.GetByGidAsync(gid);
            return new ProvinceDto
            {
                Gid = province.Gid,
                Name = province.Name,
                // Map other properties
            };
        }

        public async Task<List<string>> SearchProvinces(string query)
        {
            var provinces = await _provinceRepository.SearchByNameAsync(query);
            return provinces.Select(p => p.Name).ToList();
        }
    }
}
