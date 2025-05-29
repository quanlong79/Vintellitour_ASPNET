using Vintellitour_Framework.Models.DTOs;

namespace Vintellitour_Framework.ViewModels
{
    public class LocationViewModel
    {
        public LocationDto Location { get; set; } = new LocationDto();
        public int? ProvinceGid { get; set; }

        // Bạn có thể thêm các trường hỗ trợ cho view, ví dụ:
        // public List<PostDto> Posts { get; set; } = new();
        // public bool IsEditable { get; set; }
    }
}
