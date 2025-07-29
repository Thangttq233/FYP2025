
using System.ComponentModel.DataAnnotations; // Để dùng các thuộc tính validation

namespace FYP2025.Application.DTOs
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải dài ít nhất {2} ký tự và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc.")]
        public string FullName { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        // Mặc định cho User đăng ký là Customer. Admin sẽ được gán thủ công hoặc qua API riêng.
        // Bạn có thể thêm trường Roles nếu muốn cho phép đăng ký với role cụ thể,
        // nhưng thông thường đăng ký người dùng thông thường không có quyền chọn role.
        // public string[] Roles { get; set; } // Nếu cho phép chọn roles khi đăng ký (không khuyến khích cho user thường)
    }
}