using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP2025.Infrastructure.Data
{
    public class RefreshToken
    {
        public int Id { get; set; } 
        public string Token { get; set; } 
        public DateTime Expires { get; set; } 
        public DateTime Created { get; set; } 
        public string CreatedByIp { get; set; } 
        public DateTime? Revoked { get; set; } 
        public string? RevokedByIp { get; set; } 
        public string? ReplacedByToken { get; set; } 
        public string? ReasonRevoked { get; set; } 
        public bool IsActive => Revoked == null && Expires >= DateTime.UtcNow; 

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}