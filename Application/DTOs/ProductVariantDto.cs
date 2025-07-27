namespace FYP2025.Application.DTOs
{
    public class ProductVariantDto
    {
        public string Id { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
        public string ProductId { get; set; }
    }
}