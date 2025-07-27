namespace FYP2025.Domain.Entities
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public decimal Price { get; set; }
        //public string Size { get; set; }
        public string ImageUrl { get; set; }

        // Foreign key to Category
        public string CategoryId { get; set; }
        // Navigation property to Category
        public Category Category { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}
