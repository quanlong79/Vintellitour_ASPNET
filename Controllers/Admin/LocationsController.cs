using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;
using Vintellitour_Framework.Data;
using Vintellitour_Framework.Models.DTOs;
using Vintellitour_Framework.Models.Entities;
using Vintellitour_Framework.Services.Interfaces;
using EntitiesCoordinates = Vintellitour_Framework.Models.Entities.Coordinates;

namespace Vintellitour_Framework.Controllers.Admin
{
    [Route("admin/Locations")]
    public class LocationsController : Controller
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        // ADD: Missing Index action
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var locations = await _locationService.GetAllLocationsAsync();
                return View("~/Views/admin/Locations/Index.cshtml", locations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading locations: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách địa điểm.";
                return View("~/Views/admin/Locations/Index.cshtml", new List<LocationDto>());
            }
        }

        [HttpGet("Edit/{id?}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                var newLocation = new Location
                {
                    StreetViewUrls = new List<string>(),
                    Image = new List<string>(),
                };
                return View("~/Views/admin/Locations/Edit.cshtml", newLocation);
            }
            else
            {
                try
                {
                    var locationDto = await _locationService.GetLocationByIdAsync(id);
                    if (locationDto == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy địa điểm.";
                        return RedirectToAction("Index");
                    }
                    var locationEntity = MapToLocationEntity(locationDto);
                    return View("~/Views/admin/Locations/Edit.cshtml", locationEntity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading location {id}: {ex.Message}");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin địa điểm.";
                    return RedirectToAction("Index");
                }
            }
        }

        [HttpPost("Edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Location model, IFormFile image, List<string> StreetViewUrls)
        {
            // IMPROVED: Better debug logging
            Console.WriteLine($"=== DEBUG POST DATA ===");
            Console.WriteLine($"ID: '{id}'");
            Console.WriteLine($"Model.Name: '{model?.Name}'");
            Console.WriteLine($"Model.Address: '{model?.Address}'");
            Console.WriteLine($"Model.Description: '{model?.Description}'");
            Console.WriteLine($"Model.DescriptionHistory: '{model?.DescriptionHistory}'");
            Console.WriteLine($"Model.OpenTime: '{model?.OpenTime}'");
            Console.WriteLine($"Model.Price: '{model?.Price}'");
            Console.WriteLine($"Model.Coordinates: Lat={model?.Coordinates?.Lat}, Lng={model?.Coordinates?.Lng}");
            Console.WriteLine($"Image: {(image != null ? $"'{image.FileName}' ({image.Length} bytes)" : "null")}");

            // FIXED: Handle StreetViewUrls parameter separately
            if (StreetViewUrls != null && StreetViewUrls.Any())
            {
                model.StreetViewUrls = StreetViewUrls.Where(url => !string.IsNullOrWhiteSpace(url)).ToList();
            }
            else if (model.StreetViewUrls == null)
            {
                model.StreetViewUrls = new List<string>();
            }

            Console.WriteLine($"StreetViewUrls count: {model.StreetViewUrls?.Count ?? 0}");
            if (model.StreetViewUrls != null)
            {
                for (int i = 0; i < model.StreetViewUrls.Count; i++)
                {
                    Console.WriteLine($"  StreetViewUrls[{i}]: '{model.StreetViewUrls[i]}'");
                }
            }
            Console.WriteLine($"=== END DEBUG ===");

            // IMPROVED: Validate required fields manually if needed
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Tên địa điểm là bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError("Address", "Địa chỉ là bắt buộc.");
            }

            // IMPROVED: Initialize coordinates if null
            if (model.Coordinates == null)
            {
                model.Coordinates = new Coordinates { Lat = 0, Lng = 0 };
            }

            // Debug: Log model state
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODEL STATE ERRORS ===");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                Console.WriteLine("=== END MODEL STATE ERRORS ===");
                return View("~/Views/admin/Locations/Edit.cshtml", model);
            }

            try
            {
                // IMPROVED: Handle image upload with better error handling
                if (image != null && image.Length > 0)
                {
                    var imagePath = await UploadImageAsync(image);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        model.Image = model.Image ?? new List<string>();
                        model.Image.Add(imagePath);
                        Console.WriteLine($"Image uploaded successfully: {imagePath}");
                    }
                    else
                    {
                        Console.WriteLine("Image upload failed");
                        ModelState.AddModelError("image", "Không thể tải lên hình ảnh. Vui lòng thử lại.");
                        return View("~/Views/admin/Locations/Edit.cshtml", model);
                    }
                }

                if (string.IsNullOrEmpty(id))
                {
                    // Tạo mới
                    Console.WriteLine("Creating new location...");
                    var locationDto = MapToLocationDto(model);
                    await _locationService.AddLocationAsync(locationDto);
                    TempData["SuccessMessage"] = "Thêm địa điểm thành công!";
                    Console.WriteLine("Location created successfully");
                    return RedirectToAction("Index");
                }
                else
                {
                    // Cập nhật
                    Console.WriteLine($"Updating location with ID: {id}");
                    var existingLocationDto = await _locationService.GetLocationByIdAsync(id);
                    if (existingLocationDto == null)
                    {
                        Console.WriteLine($"Location with ID {id} not found");
                        TempData["ErrorMessage"] = "Không tìm thấy địa điểm để cập nhật.";
                        return RedirectToAction("Index");
                    }

                    // Cập nhật thông tin, giữ lại một số thông tin cũ nếu cần
                    var updatedLocationDto = MapToLocationDto(model, existingLocationDto);
                    updatedLocationDto.Id = id; // Đảm bảo ID không bị thay đổi

                    await _locationService.UpdateLocationAsync(updatedLocationDto);
                    TempData["SuccessMessage"] = "Cập nhật địa điểm thành công!";
                    Console.WriteLine("Location updated successfully");

                    // Redirect để tránh duplicate submission
                    return RedirectToAction("Edit", new { id = id });
                }
            }
            catch (Exception ex)
            {
                // IMPROVED: Better error logging
                Console.WriteLine($"Error saving location: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Có lỗi xảy ra khi lưu dữ liệu: {ex.Message}");
                return View("~/Views/admin/Locations/Edit.cshtml", model);
            }
        }

        // IMPROVED: Better mapping with null checks
        private Location MapToLocationEntity(LocationDto locationDto)
        {
            if (locationDto == null)
            {
                throw new ArgumentNullException(nameof(locationDto));
            }

            return new Location
            {
                Id = locationDto.Id,
                Name = locationDto.Name ?? string.Empty,
                Address = locationDto.Address ?? string.Empty,
                Description = locationDto.Description ?? string.Empty,
                DescriptionHistory = locationDto.DescriptionHistory ?? string.Empty,
                Slug = locationDto.Slug ?? string.Empty,
                ProvinceGid = locationDto.ProvinceGid,
                OpenTime = locationDto.OpenTime ?? string.Empty,
                Price = locationDto.Price ?? string.Empty,
                Image = locationDto.Image ?? new List<string>(),
                StreetViewUrls = locationDto.StreetViewUrls ?? new List<string>(),
                Tags = locationDto.Tags,
                Coordinates = new EntitiesCoordinates
                {
                    Lat = locationDto.Coordinates?.Lat ?? 0,
                    Lng = locationDto.Coordinates?.Lng ?? 0
                }
            };
        }

        // IMPROVED: Better mapping with validation
        private LocationDto MapToLocationDto(Location model, LocationDto existingLocationDto = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var locationDto = existingLocationDto ?? new LocationDto();

            // Cập nhật các trường cơ bản
            locationDto.Id = model.Id;
            locationDto.Name = model.Name?.Trim() ?? string.Empty;
            locationDto.Address = model.Address?.Trim() ?? string.Empty;
            locationDto.Description = model.Description?.Trim();
            locationDto.DescriptionHistory = model.DescriptionHistory?.Trim();
            locationDto.Slug = model.Slug?.Trim();
            locationDto.OpenTime = model.OpenTime?.Trim();
            locationDto.Price = model.Price?.Trim();
            locationDto.Tags = model.Tags;

            // IMPROVED: Better image handling
            if (model.Image != null && model.Image.Any(img => !string.IsNullOrWhiteSpace(img)))
            {
                locationDto.Image = model.Image.Where(img => !string.IsNullOrWhiteSpace(img)).ToList();
            }
            else if (existingLocationDto?.Image != null)
            {
                // Keep existing images if no new ones provided
                locationDto.Image = existingLocationDto.Image;
            }
            else
            {
                locationDto.Image = new List<string>();
            }

            // IMPROVED: Better StreetViewUrls handling
            if (model.StreetViewUrls != null && model.StreetViewUrls.Any())
            {
                locationDto.StreetViewUrls = model.StreetViewUrls
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url.Trim())
                    .ToList();
            }
            else
            {
                locationDto.StreetViewUrls = new List<string>();
            }

            // IMPROVED: Better coordinates handling
            if (locationDto.Coordinates == null)
            {
                locationDto.Coordinates = new LocationDto.CoordinatesDto();
            }

            if (model.Coordinates != null)
            {
                locationDto.Coordinates.Lat = model.Coordinates.Lat;
                locationDto.Coordinates.Lng = model.Coordinates.Lng;
            }

            return locationDto;
        }

        // IMPROVED: Better image upload with validation
        private async Task<string> UploadImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    Console.WriteLine($"Invalid file extension: {fileExtension}");
                    return string.Empty;
                }

                // Validate file size (e.g., max 5MB)
                if (image.Length > 5 * 1024 * 1024)
                {
                    Console.WriteLine($"File too large: {image.Length} bytes");
                    return string.Empty;
                }

                // Tạo tên file unique
                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var uploadPath = Path.Combine("wwwroot", "uploads", "locations");

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                Console.WriteLine($"Image uploaded successfully: /uploads/locations/{fileName}");
                return $"/uploads/locations/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return string.Empty;
            }
        }

        // ADD: Helper method to delete location (if needed)
        // IMPROVED: Enhanced Delete method with better validation and error handling
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Location ID is required"
                    });
                }

                // Check if location exists first
                var existingLocation = await _locationService.GetLocationByIdAsync(id);
                if (existingLocation == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Location with ID '{id}' not found"
                    });
                }

                // Delete the location
                await _locationService.DeleteLocationAsync(id);

                return Ok(new
                {
                    success = true,
                    message = $"Location '{existingLocation.Name}' has been deleted successfully",
                    deletedId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting location {id}: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while deleting location",
                    error = ex.Message
                });
            }
        }
        // IMPROVED: Add GET method for delete confirmation page (optional)
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmation(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "ID địa điểm không hợp lệ.";
                return RedirectToAction("Index");
            }

            try
            {
                var locationDto = await _locationService.GetLocationByIdAsync(id);
                if (locationDto == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy địa điểm.";
                    return RedirectToAction("Index");
                }

                var locationEntity = MapToLocationEntity(locationDto);
                return View("~/Views/admin/Locations/Delete.cshtml", locationEntity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading location for deletion {id}: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin địa điểm.";
                return RedirectToAction("Index");
            }
        }

        // IMPROVED: Helper method to delete associated images
        public async Task DeleteLocationAsync(string id)
        {
            // Find the location by ID using the Locations collection in MongoDB
            var location = await _locationService.Locations
                .Find(loc => loc.Id == id)  // Use Find method to search for the location by ID
                .FirstOrDefaultAsync();  // Get the first match or null if not found

            // If location is not found, throw an exception
            if (location == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy địa điểm với ID: {id}");
            }

            // Delete the location from the database
            var result = await _locationService.Locations
                .DeleteOneAsync(loc => loc.Id == id);  // Delete the location by ID

            // Check if the delete operation was successful
            if (result.DeletedCount == 0)
            {
                throw new InvalidOperationException($"Không thể xóa địa điểm với ID: {id}. Hành động không thành công.");
            }

            // Optional: You can log or return success here
            Console.WriteLine($"Đã xóa địa điểm với ID: {id}");
        }

        // IMPROVED: Bulk delete method (optional)
        [HttpPost("DeleteMultiple")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple(List<string> selectedIds)
        {
            Console.WriteLine($"=== BULK DELETE REQUEST ===");
            Console.WriteLine($"IDs to delete: {string.Join(", ", selectedIds ?? new List<string>())}");

            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một địa điểm để xóa.";
                return RedirectToAction("Index");
            }

            int successCount = 0;
            int errorCount = 0;
            var errorMessages = new List<string>();

            foreach (var id in selectedIds.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                try
                {
                    var existingLocation = await _locationService.GetLocationByIdAsync(id);
                    if (existingLocation == null)
                    {
                        errorCount++;
                        errorMessages.Add($"Không tìm thấy địa điểm với ID: {id}");
                        continue;
                    }


                    // Delete from database
                    await _locationService.DeleteLocationAsync(id);
                    successCount++;
                    Console.WriteLine($"Successfully deleted location: {existingLocation.Name}");
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"Error deleting location {id}: {ex.Message}");
                    errorMessages.Add($"Lỗi khi xóa địa điểm ID {id}: {ex.Message}");
                }
            }

            // IMPROVED: Detailed feedback message
            if (successCount > 0 && errorCount == 0)
            {
                TempData["SuccessMessage"] = $"Xóa thành công {successCount} địa điểm.";
            }
            else if (successCount > 0 && errorCount > 0)
            {
                TempData["SuccessMessage"] = $"Xóa thành công {successCount} địa điểm.";
                TempData["ErrorMessage"] = $"Có {errorCount} lỗi xảy ra: {string.Join("; ", errorMessages)}";
            }
            else
            {
                TempData["ErrorMessage"] = $"Không thể xóa địa điểm. Lỗi: {string.Join("; ", errorMessages)}";
            }

            Console.WriteLine($"=== END BULK DELETE REQUEST ===");
            return RedirectToAction("Index");
        }
    }
}