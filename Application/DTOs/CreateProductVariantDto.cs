using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class CreateProductVariantDto
    {
        [Required]
        [MaxLength(50)]
        public string Color { get; set; }
        [Required]
        [MaxLength(50)]
        public string Size { get; set; }
        [Range(0.01, 1000000.00)]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}