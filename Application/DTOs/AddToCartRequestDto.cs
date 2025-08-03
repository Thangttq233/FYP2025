using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class AddToCartRequestDto
    {
        [Required(ErrorMessage = "ID biến thể sản phẩm là bắt buộc.")]
        public string ProductVariantId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}