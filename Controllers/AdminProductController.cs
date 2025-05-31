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
                // Log thông tin model để debug
                System.Diagnostics.Debug.WriteLine($"Creating product: {model?.Name}");

                // Kiểm tra validation
                if (!ModelState.IsValid)
                {
                    // Tạo dictionary lỗi để trả về client
                    var errors = new Dictionary<string, string[]>();

                    foreach (var modelError in ModelState.Where(x => x.Value.Errors.Count > 0))
                    {
                        var errorMessages = modelError.Value.Errors.Select(e => e.ErrorMessage).ToArray();
                        errors.Add(modelError.Key, errorMessages);

                        // Log lỗi để debug
                        System.Diagnostics.Debug.WriteLine($"Validation Error - Field: {modelError.Key}, Errors: {string.Join(", ", errorMessages)}");
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = errors
                    });
                }

                // Thiết lập thời gian tạo và cập nhật
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;

                // Tự tạo Id nếu chưa có hoặc rỗng
                if (string.IsNullOrEmpty(model.Id))
                {
                    model.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                }

                // Gọi service lưu product
                await _productService.CreateProductAsync(model);

                System.Diagnostics.Debug.WriteLine($"Product created successfully: {model.Name}");

                // Trả về JSON thành công kèm Id mới tạo
                return Json(new
                {
                    success = true,
                    message = "Thêm sản phẩm thành công!",
                    productId = model.Id
                });
            }
            catch (ArgumentException argEx)
            {
                // Lỗi do dữ liệu sai từ service
                System.Diagnostics.Debug.WriteLine($"Argument Error: {argEx.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = argEx.Message
                });
            }
            catch (InvalidOperationException opEx)
            {
                // Lỗi logic nghiệp vụ
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

                // Có thể ghi log ở đây hoặc lưu vào file log nếu có hệ thống logging

                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi thêm sản phẩm. Vui lòng thử lại sau.",
                    details = ex.Message  // Nếu môi trường development thì show thêm lỗi chi tiết
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

                return View(product);
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
                return View(model);
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
                return View(model);
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

                return View(product);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product for delete {id}: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi tải thông tin sản phẩm" });
            }
        }

        // Route: /admin/product/delete/{id} (POST)
        [HttpPost("delete/{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "ID sản phẩm không hợp lệ" });
            }

            try
            {
                await _productService.DeleteProductAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting product {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa sản phẩm";
                return RedirectToAction(nameof(Index));
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