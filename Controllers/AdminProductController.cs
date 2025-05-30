using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Vintellitour_Framework.ViewModels;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;
using System.Linq;

namespace Vintellitour_Framework.Controllers.Admin
{
    [Route("admin/product")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;  // Bạn cần tạo interface và service cho product

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("admin")]
        [HttpGet("/admin/product")]
        public async Task<IActionResult> Index()
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

        // GET: /Admin/Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _productService.CreateProductAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Product/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: /Admin/Product/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            await _productService.UpdateProductAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Product/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: /Admin/Product/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
