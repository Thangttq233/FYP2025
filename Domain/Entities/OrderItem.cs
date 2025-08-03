namespace FYP2025.Domain.Entities
{
    public class OrderItem
    {
        public string Id { get; set; } 
        public string OrderId { get; set; } 
        public string ProductVariantId { get; set; } 
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } 

        public string ProductSnapshotName { get; set; } // Tên sản phẩm tại thời điểm đặt hàng
        public string ProductVariantSnapshotColor { get; set; } // Màu biến thể tại thời điểm đặt hàng
        public string ProductVariantSnapshotSize { get; set; } // Kích thước biến thể tại thời điểm đặt hàng
        public string ProductVariantSnapshotImageUrl { get; set; } // Ảnh biến thể tại thời điểm đặt hàng

        public Order Order { get; set; }
        public ProductVariant ProductVariant { get; set; } 
    }
}