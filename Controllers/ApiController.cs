using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Services.Interfaces;

namespace Vintellitour_Framework.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly ILocationService _locationService;

        public ApiController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations(int gid)
        {
            var locations = await _locationService.GetLocationsByProvinceGid(gid);
            return Ok(new { data = locations });
        }
    }
}
