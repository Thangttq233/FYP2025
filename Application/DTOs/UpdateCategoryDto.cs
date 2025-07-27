using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public required string Name { get; set; }
    }
}
