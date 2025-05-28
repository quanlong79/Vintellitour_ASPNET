using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> InStockItems { get; set; } = new();
        public List<CartItemViewModel> OutOfStockItems { get; set; } = new();

        public decimal Total => InStockItems.Sum(x => x.Product.Price * x.Quantity);
        public decimal Discount => InStockItems.Sum(x => (x.Product.OriginalPrice - x.Product.Price) * x.Quantity);

        public decimal GrandTotal => Total;
    }
}
