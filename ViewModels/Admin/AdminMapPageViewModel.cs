// File: ViewModels/Admin/AdminMapPageViewModel.cs
namespace Vintellitour_Framework.Web.ViewModels.Admin
{
    public class AdminMapPageViewModel
    {
        /// <summary>
        /// Đường dẫn đến file GeoJSON chứa thông tin ranh giới các tỉnh.
        /// Ví dụ: "/data/province.json"
        /// </summary>
        public string? ProvinceGeoJsonPath { get; set; }

        /// <summary>
        /// Vĩ độ ban đầu của trung tâm bản đồ.
        /// </summary>
        public double InitialLat { get; set; } = 14.0583; // Mặc định là Việt Nam

        /// <summary>
        /// Kinh độ ban đầu của trung tâm bản đồ.
        /// </summary>
        public double InitialLng { get; set; } = 108.2772; // Mặc định là Việt Nam

        /// <summary>
        /// Mức zoom ban đầu của bản đồ.
        /// </summary>
        public int InitialZoom { get; set; } = 6; // Mức zoom tổng quan Việt Nam

        // Bạn có thể thêm các thuộc tính khác nếu cần cho trang bản đồ admin,
        // ví dụ: API key cho tile layer nếu admin dùng key khác,
        // danh sách các lớp layer tùy chọn, v.v.
    }
}