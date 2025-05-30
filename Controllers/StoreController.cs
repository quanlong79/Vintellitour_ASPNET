using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Models;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;

namespace Vintellitour_Framework.Controllers
{
    public class StoreController : Controller
    {
        private readonly ProductService _productService;

        public StoreController(ProductService productService)
        {
            _productService = productService;
        }

        // Action trả về View (trang chính)
        public async Task<IActionResult> Index(string category, int? minPrice, int? maxPrice)
        {
            // Nếu không có khoảng giá, đặt mặc định
            int min = minPrice ?? 0;
            int max = maxPrice ?? 2000000;

            var products = await _productService.GetFilteredProductsAsync(category, min, max);

            ViewBag.SelectedCategory = category;
            ViewBag.MinPrice = min;
            ViewBag.MaxPrice = max;

            return View(products);
        }

        // API endpoint cho AJAX calls từ JavaScript
        [HttpGet]
        public async Task<IActionResult> GetProducts(string category = "", int maxPrice = 2000000, string search = "", string sort = "featured")
        {
            try
            {
                // Gọi filter từ MongoDB, bao gồm cả Regex ignore-case với category
                var products = await _productService.GetFilteredProductsAsync(category, 0, maxPrice);

                // Lọc thêm theo từ khóa tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    products = products.Where(p =>
                        p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Description.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Sắp xếp
                switch (sort.ToLower())
                {
                    case "newest":
                        products = products.OrderByDescending(p => p.CreatedAt).ToList();
                        break;
                    case "price_asc":
                        products = products.OrderBy(p => p.Price).ToList();
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => p.Price).ToList();
                        break;
                    case "rating":
                        products = products.OrderByDescending(p => p.Rating).ToList();
                        break;
                    case "featured":
                    default:
                        products = products.OrderByDescending(p => p.IsBestSeller)
                                           .ThenByDescending(p => p.IsNew)
                                           .ThenByDescending(p => p.Rating)
                                           .ToList();
                        break;
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(products);
                }

                return View("Index", products);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProducts: {ex.Message}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { error = "Có lỗi xảy ra khi tải sản phẩm" });
                }

                return View("Index", new List<Product>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return Json(product);
        }


        // API endpoint để lấy chi tiết sản phẩm (nếu cần)
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { error = "Không tìm thấy sản phẩm" });
                }

                return Json(product);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProductDetails: {ex.Message}");
                return BadRequest(new { error = "Có lỗi xảy ra" });
            }
        }
    }
}