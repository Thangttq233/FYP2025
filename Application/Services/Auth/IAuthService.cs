using FYP2025.Application.DTOs;
using System.Threading.Tasks;

namespace FYP2025.Application.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress); 
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress); 
        Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress); 
        Task<AuthResponseDto> AssignRoleToUserAsync(string email, string roleName);
        Task<AuthResponseDto> RemoveRoleFromUserAsync(string email, string roleName);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<int> GetTotalUsersAsync();
    }
}