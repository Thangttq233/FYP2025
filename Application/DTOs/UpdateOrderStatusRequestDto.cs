using System.ComponentModel.DataAnnotations;
using FYP2025.Domain.Entities; 

namespace FYP2025.Application.DTOs
{
    public class UpdateOrderStatusRequestDto
    {
        [Required(ErrorMessage = "ID đơn hàng là bắt buộc.")]
        public string OrderId { get; set; }

        [Required(ErrorMessage = "Trạng thái mới là bắt buộc.")]
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "Trạng thái đơn hàng không hợp lệ.")]
        public OrderStatus NewStatus { get; set; }
    }
}