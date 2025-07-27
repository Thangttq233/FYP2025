namespace FYP2025.Domain.Entities
{
    public class ProductVariant
    {
        public string Id { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; } 
         public string ImageUrl { get; set; } 
        public required string ProductId { get; set; } 
        public Product Product { get; set; } = null!; 
    }
}
