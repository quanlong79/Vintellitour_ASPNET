using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Vintellitour_Framework.Services.Interfaces;
using Vintellitour_Framework.Models.Entities; // Đảm bảo có namespace chứa class Location
using Vintellitour_Framework.Models.DTOs; // Thêm namespace chứa class LocationDto

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

        // GET: api/location?gid=1
        [HttpGet]
        public async Task<IActionResult> GetLocationsByProvince([FromQuery] int gid)
        {
            try
            {
                var locations = await _locationService.GetLocationsByProvinceGid(gid);

                var response = new
                {
                    success = true,
                    data = locations,
                    message = "Success"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    data = new object[] { },
                    message = ex.Message
                });
            }
        }

        // POST: api/location
        [HttpPost]
        public async Task<IActionResult> AddLocation([FromBody] Location location)
        {
            if (location == null)
                return BadRequest(new { error = "Location data is required" });

            // Chuyển từ Entity Location sang LocationDto
            var locationDto = MapToLocationDto(location);

            var result = await _locationService.AddLocationAsync(locationDto); // Thêm LocationDto vào DB

            return Ok(new { success = true, data = result });
        }

        // Chuyển từ Location Entity thành LocationDto
        private LocationDto MapToLocationDto(Location location)
        {
            return new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                Description = location.Description,
                DescriptionHistory = location.DescriptionHistory,
                Slug = location.Slug,
                ProvinceGid = location.ProvinceGid,
                OpenTime = location.OpenTime,
                Price = location.Price,
                StreetViewUrls = location.StreetViewUrls,
                Tags = location.Tags,
                Image = location.Image,
                Coordinates = new LocationDto.CoordinatesDto
                {
                    Lat = location.Coordinates?.Lat ?? 0,
                    Lng = location.Coordinates?.Lng ?? 0
                }
            };
        }
    }
}
