
using System.ComponentModel.DataAnnotations; // Để sử dụng [Required] và [EmailAddress]

namespace FYP2025.Application.DTOs
{
    public class AssignRoleRequestDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tên vai trò là bắt buộc.")]
        public string RoleName { get; set; } // Ví dụ: "Admin", "Customer", "Saler"
    }
}