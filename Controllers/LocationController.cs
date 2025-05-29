using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vintellitour_Framework.Services.Interfaces;

namespace Vintellitour_Framework.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        // GET: api/locations?gid=1
        [HttpGet]
        public async Task<IActionResult> GetLocationsByProvince([FromQuery] int gid)
        {
            try
            {
                var locations = await _locationService.GetLocationsByProvinceGid(gid);

                // Trả về format giống như trong JavaScript expect
                var response = new
                {
                    success = true,
                    data = locations,
                    message = "Success"
                };

                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    data = new object[] { },
                    message = ex.Message
                });
            }
        }
    }
}