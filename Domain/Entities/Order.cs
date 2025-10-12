using FYP2025.Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace FYP2025.Domain.Entities
{
    public enum OrderStatus
    {
        Pending,        
        Processing,     
        Shipped,        
        Delivered,      
        Cancelled       
    }

    public enum PaymentStatus
    {
        Unpaid,
        Paid,
        Failed,
        Refunded
    }

    public class Order
    {
        public string Id { get; set; } // ID của đơn hàng
        public string UserId { get; set; } // Khóa ngoại đến ApplicationUser (sẽ được cấu hình sau)
        public ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending; // Trạng thái mặc định
        public decimal TotalPrice { get; set; } // Tổng giá trị đơn hàng
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        // Thông tin vận chuyển/địa chỉ nhận hàng (tùy thuộc vào thiết kế chi tiết hơn)
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CustomerName { get; set; } // Lấy từ user FullName

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}