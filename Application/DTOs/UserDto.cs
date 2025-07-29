namespace FYP2025.Application.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        // Có thể thêm các thuộc tính khác từ ApplicationUser mà bạn muốn hiển thị
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        // ...
    }
}