using Microsoft.AspNetCore.Identity;

namespace FYP2025.Infrastructure.Data
{
    public class ApplicationUser : IdentityUser
    {
        // Thêm các thuộc tính tùy chỉnh của bạn vào đây (ví dụ: FirstName, LastName, BirthDate, Address)
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        // public string Address { get; set; } // Ví dụ
    }
}