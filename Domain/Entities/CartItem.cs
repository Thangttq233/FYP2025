namespace FYP2025.Domain.Entities
{
    public class CartItem
    {
        public string Id { get; set; } 
        public string CartId { get; set; } 
        public string ProductVariantId { get; set; } 
        public int Quantity { get; set; }

        public Cart Cart { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}