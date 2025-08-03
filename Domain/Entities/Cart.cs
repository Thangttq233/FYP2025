
using System.Collections.Generic;

namespace FYP2025.Domain.Entities
{
    public class Cart
    {
        public string Id { get; set; } 
        public string UserId { get; set; } 

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}