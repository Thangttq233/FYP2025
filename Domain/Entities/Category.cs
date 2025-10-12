using System.Collections.Generic;
using FYP2025.Domain.Enums;

namespace FYP2025.Domain.Entities
{
    public class Category
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public MainCategoryType MainCategory { get; set; }


        // Quan hệ 1 - nhiều với Product
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
