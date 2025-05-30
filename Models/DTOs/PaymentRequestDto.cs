namespace Vintellitour_Framework.Models.DTOs
{
    public class OrderItemDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
    }

    public class momoResponsePaymentRequestDto
    {
        public string FullName { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; }
        public string CartItemsJson { get; set; }
    }
    public class PaymentRequestDto
    {
        public string FullName { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; }
        public string CartItemsJson { get; set; }
    }
}
