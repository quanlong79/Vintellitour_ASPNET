namespace Vintellitour_Framework.ViewModels
{
    public class ProductViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } 
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; } // bạn có thể gán icon tương ứng category
        public int StockQuantity { get; set; }
        public double Rating { get; set; }
        public int Reviews { get; set; }
        public bool IsNew { get; set; }
        public bool IsBestSeller { get; set; }
    }
}
