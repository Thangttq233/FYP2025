using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class UpdateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [Required]
        public required string CategoryId { get; set; }
        public IFormFile? ImageFile { get; set; }

        public ICollection<UpdateProductVariantDto> Variants { get; set; } = new List<UpdateProductVariantDto>();
    }
}
