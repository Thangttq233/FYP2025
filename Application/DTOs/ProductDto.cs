namespace FYP2025.Application.DTOs
{
    public class ProductDto
    {
        public string Id { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryId { get; set; } 
        public string CategoryName { get; set; }

        public ICollection<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
    }
}
