namespace FYP2025.Application.DTOs
{
    public class AuthResponseDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; } // Danh sách các vai trò của người dùng
        public string Token { get; set; } // JWT Token
        public DateTime Expiration { get; set; } // Thời gian hết hạn của Token
        public bool IsSuccess { get; set; } = true;
        public List<string> Errors { get; set; } // Danh sách lỗi nếu đăng nhập/đăng ký thất bại
        public string RefreshToken { get; set; }
    }
}