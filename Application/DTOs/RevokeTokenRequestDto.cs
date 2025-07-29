using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class RevokeTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}