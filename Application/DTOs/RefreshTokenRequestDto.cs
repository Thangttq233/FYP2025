
using System.ComponentModel.DataAnnotations;

namespace FYP2025.Application.DTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}