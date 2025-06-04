using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Services.Geospatial;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class GeospatialController : ControllerBase
{
    [HttpPost("check-point")]
    public IActionResult CheckPointInProvince([FromBody] CheckPointRequest request)
    {
        try
        {
            // Đọc file GeoJSON từ ổ cứng (hoặc bạn có thể cache nội dung)
            var geoJsonContent = System.IO.File.ReadAllText("Data/ha_noi_polygon.geojson");

            var polygon = GeoJsonParser.ParsePolygonCoordinates(geoJsonContent);

            bool isInside = CustomGeospatialService.IsPointInPolygon(request.Lat, request.Lng, polygon);

            return Ok(new { IsInside = isInside });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}

public class CheckPointRequest
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}
