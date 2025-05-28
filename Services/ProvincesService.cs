using Vintellitour_Framework.Data.Repositories;
using Vintellitour_Framework.Models.DTOs;
using Vintellitour_Framework.Services.Interfaces;

namespace YourNamespace.Services
{
    public class ProvinceService : IProvinceService
    {
        private readonly IProvinceRepository _provinceRepository;

        public ProvinceService(IProvinceRepository provinceRepository)
        {
            _provinceRepository = provinceRepository;
        }

        public async Task<ProvinceDto?> GetProvinceByGid(int gid)
        {
            var province = await _provinceRepository.GetByGidAsync(gid);
            if (province == null) return null;

            return new ProvinceDto
            {
                Gid = province.Gid,
                Name = province.Name,
                // Map thêm các trường nếu cần
            };
        }

        public async Task<List<string>> SearchProvinces(string query)
        {
            var provinces = await _provinceRepository.SearchByNameAsync(query);
            return provinces.Select(p => p.Name).ToList();
        }
    }
}
