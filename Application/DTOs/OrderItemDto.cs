namespace FYP2025.Application.DTOs
{
    public class OrderItemDto
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string ProductVariantId { get; set; } 
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string ProductSnapshotName { get; set; }
        public string ProductVariantSnapshotColor { get; set; }
        public string ProductVariantSnapshotSize { get; set; }
        public string ProductVariantSnapshotImageUrl { get; set; }
    }
}