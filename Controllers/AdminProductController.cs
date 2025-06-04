using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Vintellitour_Framework.ViewModels;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;

namespace Vintellitour_Framework.Controllers.Admin
{
    [Route("admin/product")]
    public class AdminProductController : Controller
    {
        private readonly IProductService _productService;

        public AdminProductController(IProductService productService)
        {
            _productService = productService;
        }

        // Route: /admin/product
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();

                var model = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.Image,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    CategoryName = p.Category,
                    CategoryIcon = GetCategoryIcon(p.Category),
                    StockQuantity = p.Stock,
                    Rating = p.Rating,
                    Reviews = p.Reviews,
                    IsNew = p.IsNew,
                    IsBestSeller = p.IsBestSeller
                }).ToList();

                return View("~/Views/admin/products.cshtml", model);
            }
            catch (Exception ex)
            {
                // Log error và hiển thị trang lỗi
                System.Diagnostics.Debug.WriteLine($"Error in Index: {ex.Message}");
                return View("Error");
            }
        }

        private string GetCategoryIcon(string category)
        {
            return category switch
            {
                "Balo" => "🎒",
                "Bản đồ" => "🗺️",
                "Phụ kiện" => "📦",
                "Điện tử" => "📱",
                _ => "📦",
            };
        }

        // Route: /admin/product/create (GET)
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("~/Views/admin/Product/Create.cshtml");
        }

        // Route: /admin/product/create (POST)
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            try
            {
                // Loại bỏ lỗi validation liên quan tới Id vì client không gửi hoặc gửi sai
                ModelState.Remove(nameof(model.Id));

                if (!ModelState.IsValid)
                {
                    var errors = new Dictionary<string, string[]>();

                    foreach (var modelError in ModelState.Where(x => x.Value.Errors.Count > 0))
                    {
                        var errorMessages = modelError.Value.Errors.Select(e => e.ErrorMessage).ToArray();
                        errors.Add(modelError.Key, errorMessages);

                        System.Diagnostics.Debug.WriteLine($"Validation Error - Field: {modelError.Key}, Errors: {string.Join(", ", errorMessages)}");
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = errors
                    });
                }

                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(model.Id))
                {
                    model.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                }

                await _productService.CreateProductAsync(model);

                System.Diagnostics.Debug.WriteLine($"Product created successfully: {model.Name}");

                return Json(new
                {
                    success = true,
                    message = "Thêm sản phẩm thành công!"
                });
            }
            catch (ArgumentException argEx)
            {
                System.Diagnostics.Debug.WriteLine($"Argument Error: {argEx.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = argEx.Message
                });
            }
            catch (InvalidOperationException opEx)
            {
                System.Diagnostics.Debug.WriteLine($"Operation Error: {opEx.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = opEx.Message
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"System Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi thêm sản phẩm. Vui lòng thử lại sau.",
                    details = ex.Message
                });
            }
        }



        // Route: /admin/product/edit/{id} (GET)
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(new { message = "ID sản phẩm không hợp lệ" });
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm" });
                }

                return View("~/Views/admin/Product/Edit.cshtml", product);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product {id}: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi tải thông tin sản phẩm" });
            }
        }

        // Route: /admin/product/edit/{id} (POST)
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "ID sản phẩm không hợp lệ" });
            }

            if (id != model.Id)
            {
                return BadRequest(new { message = "ID không khớp" });
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/admin/Product/Edit.cshtml", model);
            }

            try
            {
               
                model.UpdatedAt = DateTime.UtcNow;
                await _productService.UpdateProductAsync(model);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating product {id}: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sản phẩm");
                return View("~/Views/admin/Product/Edit.cshtml", model);
            }
        }

        // Route: /admin/product/delete/{id} (GET)
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(new { message = "ID sản phẩm không hợp lệ" });
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm" });
                }

                return View("~/Views/admin/Product/Delete.cshtml", product);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product for delete {id}: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi tải thông tin sản phẩm" });
            }
        }

        // Route: /admin/product/delete/{id} (POST)
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { success = false, message = "ID sản phẩm không hợp lệ" });
            }

            try
            {
                await _productService.DeleteProductAsync(id);
                return Json(new { success = true, message = "Xóa sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting product {id}: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa sản phẩm" });
            }
        }


        // API endpoint để lấy thông tin sản phẩm (dùng cho AJAX)
        [HttpGet("api/{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "ID không hợp lệ" });
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm" });
                }

                return Json(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product API {id}: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Lỗi server" });
            }
        }
    }
}