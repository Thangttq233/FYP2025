using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class UpdateCartItemRequestDto
    {
        [Required(ErrorMessage = "ID mục giỏ hàng là bắt buộc.")]
        public string CartItemId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}