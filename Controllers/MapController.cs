using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vintellitour_Framework.Models.DTOs;
using Vintellitour_Framework.Services.Interfaces;

namespace Vintellitour_Framework.Controllers
{
    public class MapController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
        private readonly IProvinceService _provinceService;

        public MapController(IProvinceService provinceService)
        {
            _provinceService = provinceService;
        }


        public async Task<IActionResult> Province(int gid)
        {
            ProvinceDto? province = await _provinceService.GetProvinceByGid(gid);
            if (province == null)
                return NotFound();

            return View(province);
        }
    }
}
