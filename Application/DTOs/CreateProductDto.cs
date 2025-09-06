
using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class CreateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [Required]
        public required string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public IFormFile ImageFile { get; set; }
        public ICollection<CreateProductVariantDto>? Variants { get; set; } = new List<CreateProductVariantDto>();
    }
}
