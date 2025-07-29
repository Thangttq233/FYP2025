
using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Tên đăng nhập (Email) là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")] // Hoặc [Phone] nếu bạn cho phép đăng nhập bằng số điện thoại
        public string Email { get; set; } // Hoặc UserName nếu bạn muốn đăng nhập bằng UserName

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}