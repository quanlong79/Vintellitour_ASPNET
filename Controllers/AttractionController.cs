using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Models;
using MongoDB.Driver;

namespace Vintellitour_Framework.Controllers
{
    public class AttractionController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public AttractionController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

       
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Id không được để trống.");

            var locationsCollection = _mongoDbService.GetLocationsCollection();

            var attraction = await locationsCollection.Find(l => l.Id == id).FirstOrDefaultAsync();

            if (attraction == null)
                return NotFound("Không tìm thấy địa điểm.");
            return View(attraction);
        }
    }
}
