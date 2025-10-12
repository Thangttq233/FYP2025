using System.ComponentModel.DataAnnotations;
using FYP2025.Domain.Enums;

namespace FYP2025.Application.DTOs
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Main category type is required.")]
        public MainCategoryType MainCategory { get; set; }
    }
}
