using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CreatePostViewModel
{
    [Required]
    public string Content { get; set; }

    public string LocationRawId { get; set; }

    public int ProvinceGid { get; set; }

    // Lấy file upload từ form
    public List<IFormFile> UploadedFiles { get; set; }
}
