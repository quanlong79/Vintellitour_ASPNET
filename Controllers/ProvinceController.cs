using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using Vintellitour_Framework.ViewModels;

namespace Vintellitour_Framework.Controllers
{
    public class ProvinceController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        // Inject MongoDbService qua constructor
        public ProvinceController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // Action Details nhận param provinceId (hoặc gid) để tìm province và locations
        public async Task<IActionResult> Details(int provinceGid)
        {
            // Lấy collection provinces và locations
            var provincesCollection = _mongoDbService.GetProvincesCollection();
            var locationsCollection = _mongoDbService.GetLocationsCollection();

            // Tìm province theo provinceGid
            var province = await provincesCollection.Find(p => p.Id == provinceGid).FirstOrDefaultAsync();

            if (province == null)
            {
                return NotFound("Không tìm thấy tỉnh thành.");
            }

            // Tìm locations thuộc province này
            var locations = await locationsCollection.Find(l => l.ProvinceGid == provinceGid).ToListAsync();

                 // Tạo ViewModel đổ dữ liệu xuống View
            var viewModel = new ProvinceWithLocationsViewModel
            {
                Province = province,
                Locations = locations
            };

            return View(viewModel);
        }
    }
}
