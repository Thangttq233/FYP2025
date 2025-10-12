using FYP2025.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class CategoryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Main category type is required.")]
        public MainCategoryType MainCategory { get; set; }
    }
}
