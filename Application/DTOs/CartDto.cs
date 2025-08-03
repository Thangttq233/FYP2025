using System.Collections.Generic;

namespace FYP2025.Application.DTOs
{
    public class CartDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalCartPrice { get; set; } 
    }
}