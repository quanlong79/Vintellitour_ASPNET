// File: ViewModels/Admin/LocationFormAdminViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vintellitour_Framework.Web.ViewModels.Admin
{
    public class LocationFormAdminViewModel
    {
        /// <summary>
        /// ID của địa điểm, dùng khi chỉnh sửa. Sẽ là null hoặc empty khi tạo mới.
        /// </summary>
        public string? Id { get; set; }

        [Required(ErrorMessage = "Tên địa điểm không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên địa điểm không được vượt quá 200 ký tự.")]
        [Display(Name = "Tên địa điểm")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vĩ độ không được để trống.")]
        [Range(-90.0, 90.0, ErrorMessage = "Vĩ độ không hợp lệ.")]
        [Display(Name = "Vĩ độ (Latitude)")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ không được để trống.")]
        [Range(-180.0, 180.0, ErrorMessage = "Kinh độ không hợp lệ.")]
        [Display(Name = "Kinh độ (Longitude)")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
        [Display(Name = "Tỉnh/Thành phố")]
        public int ProvinceGid { get; set; } // Sẽ được dùng để hiển thị dropdown các tỉnh

        [StringLength(250, ErrorMessage = "Slug không được vượt quá 250 ký tự.")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ chứa chữ thường, số và dấu gạch ngang, không có khoảng trắng hoặc ký tự đặc biệt ở đầu/cuối.")]
        [Display(Name = "Slug (Đường dẫn thân thiện)")]
        public string? Slug { get; set; }

        [Display(Name = "Giờ mở cửa")]
        [StringLength(100, ErrorMessage = "Thông tin giờ mở cửa không được vượt quá 100 ký tự.")]
        public string? OpenTime { get; set; }

        [Display(Name = "Giá vé/Chi phí")]
        [StringLength(100, ErrorMessage = "Thông tin giá vé không được vượt quá 100 ký tự.")]
        public string? Price { get; set; }

        [Display(Name = "URLs Hình ảnh")]
        public List<string>? ImageUrls { get; set; } = new List<string>(); // Khởi tạo để tránh null

        [Display(Name = "Tags (Từ khóa)")]
        public List<string>? Tags { get; set; } = new List<string>(); // Khởi tạo để tránh null

        [Display(Name = "Lịch sử mô tả (nếu có)")]
        public string? DescriptionHistory { get; set; }

        [Display(Name = "Hiển thị công khai")]
        public bool IsPublished { get; set; } = true; // Mặc định là true khi tạo mới, admin có thể thay đổi

        // Constructor (nếu cần khởi tạo giá trị mặc định phức tạp hơn)
        // public LocationFormAdminViewModel()
        // {
        //     ImageUrls = new List<string>();
        //     Tags = new List<string>();
        //     IsPublished = true;
        // }
    }
}