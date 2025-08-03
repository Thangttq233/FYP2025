namespace FYP2025.Application.DTOs
{
    public class CartItemDto
    {
        public string Id { get; set; }
        public string CartId { get; set; }
        public string ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string ProductVariantColor { get; set; }
        public string ProductVariantSize { get; set; }
        public decimal ProductVariantPrice { get; set; }
        public string ProductVariantImageUrl { get; set; } 
    }
}