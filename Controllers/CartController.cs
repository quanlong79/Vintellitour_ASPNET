using Microsoft.AspNetCore.Mvc;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.ViewModels;

namespace Vintellitour_Framework.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ProductService _productService;
        private readonly PaymentService _paymentService;
        public CartController(CartService cartService, ProductService productService, PaymentService paymentService)
        {
            _cartService = cartService;
            _productService = productService;
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            await _cartService.AddToCartAsync(userId, productId, quantity);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _cartService.GetCartByUserIdAsync(userId) ?? new Cart();
            var products = await _productService.GetAllProductsAsync();

            var inStock = new List<CartItemViewModel>();
            var outOfStock = new List<CartItemViewModel>();

            foreach (var item in cart.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null) continue;

                var modelItem = new CartItemViewModel { Product = product, Quantity = item.Quantity };
                if (product.Stock > 0) inStock.Add(modelItem);
                else outOfStock.Add(modelItem);
            }

            var viewModel = new CartViewModel
            {
                InStockItems = inStock,
                OutOfStockItems = outOfStock
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Json(new { count = 0 });

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            var count = cart?.Items?.Sum(i => i.Quantity) ?? 0;

            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> IncreaseQuantity(string id)
        {
            var userId = GetUserId();
            await _cartService.IncreaseQuantityAsync(userId, id);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(string id)
        {
            var userId = GetUserId();
            await _cartService.DecreaseQuantityAsync(userId, id);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string id)
        {
            var userId = GetUserId();
            await _cartService.RemoveItemAsync(userId, id);
            return Ok();
        }

        private string GetUserId()
        {
            return HttpContext.Session.GetString("UserId");
        }
        [HttpGet("/Cart/PaymentResult")]
        public async Task<IActionResult> PaymentResult(string orderId)
        {
            var payment = await _paymentService.GetByIdAsync(orderId);

            if (payment == null)
                return RedirectToAction("Index", "Map");

            if (payment.Status == "Success")
            {
                TempData["Message"] = "Thanh toán thành công!";
            }
            else if (payment.Status == "Cancel")
            {
                TempData["Message"] = "Thanh toán thất bại hoặc bị hủy.";
            }
            else
            {
                TempData["Message"] = "Chờ xác nhận thanh toán...";
            }

            return RedirectToAction("Index", "Map"); // chuyển về giỏ hàng
        }
    }
}
