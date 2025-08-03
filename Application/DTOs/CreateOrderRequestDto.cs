using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class CreateOrderRequestDto
    {
        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc.")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string PhoneNumber { get; set; }

        public string CustomerNotes { get; set; }
    }
}