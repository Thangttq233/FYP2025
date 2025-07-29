using FYP2025.Application.DTOs;
using FYP2025.Application.Services.Auth; 
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq; 
using System.Security.Claims; 
using FYP2025.Application.Common; 
using Microsoft.AspNetCore.Authorization; 

namespace FYP2025.Api.Features.Auth 
{
    [ApiController]
    [Route("api/auth")] 
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            var ipAddress = GetIpAddress(); 
            var result = await _authService.RegisterAsync(request, ipAddress);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(new { Errors = result.Errors });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            var ipAddress = GetIpAddress(); 
            var result = await _authService.LoginAsync(request, ipAddress);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return Unauthorized(new { Errors = result.Errors }); 
        }

        // POST: api/auth/refresh (Để đổi Access Token mới từ Refresh Token)
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var ipAddress = GetIpAddress(); // Lấy IP Address của client
            var result = await _authService.RefreshTokenAsync(request, ipAddress);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(new { Errors = result.Errors });
        }

        // POST: api/auth/revoke (Để thu hồi Refresh Token khi logout)
        // Cần JWT token hợp lệ để gọi API này
        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequestDto request) 
        {
            var refreshToken = request.RefreshToken;


            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Refresh Token là bắt buộc.");

            var ipAddress = GetIpAddress();
            var result = await _authService.RevokeRefreshTokenAsync(refreshToken, ipAddress);

            if (result)
            {
                return Ok(new { message = "Refresh Token đã được thu hồi thành công." });
            }
            return BadRequest(new { message = "Không thể thu hồi Refresh Token." });
        }


        // Phương thức trợ giúp để lấy IP Address
        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "::1"; 
        }

        // GET: api/auth/check (ví dụ: một API cần xác thực để kiểm tra token)
        // Chỉ những người dùng đã đăng nhập (có token hợp lệ) mới có thể truy cập
        [HttpGet("check")]
        [Authorize] 
        public IActionResult CheckAuthentication()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            return Ok(new
            {
                Message = "Xác thực thành công!",
                UserId = userId,
                UserName = userName,
                Email = userEmail,
                Roles = roles
            });
        }

        // POST: api/auth/assign-role
        // API để gán vai trò cho người dùng (Chỉ Admin được dùng)
        [HttpPost("assign-role")]
        [Authorize(Roles = nameof(RolesEnum.Admin))] 
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.AssignRoleToUserAsync(request.Email, request.RoleName);
            if (result.IsSuccess)
            {
                return Ok(new { Message = $"Vai trò '{request.RoleName}' đã được gán cho người dùng '{request.Email}' thành công." });
            }
            return BadRequest(new { Errors = result.Errors });
        }

        // POST: api/auth/remove-role
        // API để xóa vai trò khỏi người dùng (Chỉ Admin được dùng)
        [HttpPost("remove-role")]
        [Authorize(Roles = nameof(RolesEnum.Admin))] 
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleRequestDto request) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RemoveRoleFromUserAsync(request.Email, request.RoleName);
            if (result.IsSuccess)
            {
                return Ok(new { Message = $"Vai trò '{request.RoleName}' đã được xóa khỏi người dùng '{request.Email}' thành công." });
            }
            return BadRequest(new { Errors = result.Errors });
        }

        // GET: api/auth/{userId}/roles
        // API để lấy vai trò của người dùng (Chỉ Admin hoặc user đó mới được xem)
        [HttpGet("{userId}/roles")]
        [Authorize(Roles = nameof(RolesEnum.Admin))] 
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var roles = await _authService.GetUserRolesAsync(userId);
            if (roles == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }
            return Ok(roles);
        }


        // GET: api/auth/admin-only 
        [HttpGet("admin-only")]
        [Authorize(Roles = nameof(RolesEnum.Admin))] 
        public IActionResult AdminOnly()
        {
            return Ok("Chào mừng Admin! Bạn có quyền truy cập tài nguyên Admin.");
        }

        // GET: api/auth/saler-only 
        [HttpGet("saler-only")]
        [Authorize(Roles = nameof(RolesEnum.Saler))] 
        public IActionResult SalerOnly()
        {
            return Ok("Chào mừng Saler! Bạn có quyền truy cập tài nguyên Saler.");
        }

        // GET: api/auth/customer-only 
        [HttpGet("customer-only")]
        [Authorize(Roles = nameof(RolesEnum.Customer))] 
        public IActionResult CustomerOnly()
        {
            return Ok("Chào mừng Customer! Bạn có quyền truy cập tài nguyên Customer.");
        }
    }
}