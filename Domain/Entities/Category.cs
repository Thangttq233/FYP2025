using System.Collections.Generic;

namespace FYP2025.Domain.Entities
{
    public class Category
    {
        public string Id { get; set; }
        public required string Name { get; set; }

        // Quan hệ 1 - nhiều với Product
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
