﻿using FYP2025.Application.DTOs;
using FYP2025.Infrastructure.Data; 
using Microsoft.AspNetCore.Identity; 
using Microsoft.Extensions.Configuration; 
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using System.Text; 
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq; 
using FYP2025.Application.Common; 
using Microsoft.EntityFrameworkCore;

namespace FYP2025.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext; 

        public AuthService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           RoleManager<ApplicationRole> roleManager,
                           IConfiguration configuration,
                           ApplicationDbContext dbContext) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _dbContext = dbContext; 
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress)
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Email đã tồn tại." }
                };
            }

            var newUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (result.Succeeded)
            {
                string customerRoleName = RolesEnum.Customer.ToString();
                if (!await _roleManager.RoleExistsAsync(customerRoleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole { Name = customerRoleName, NormalizedName = customerRoleName.ToUpper() });
                }
                await _userManager.AddToRoleAsync(newUser, customerRoleName);

                var authResponse = await GenerateJwtToken(newUser, ipAddress);
                authResponse.IsSuccess = true;
                return authResponse;
            }

            return new AuthResponseDto
            {
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Email hoặc mật khẩu không đúng." }
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {       
                var authResponse = await GenerateJwtToken(user, ipAddress); 
                authResponse.IsSuccess = true;
                return authResponse;
            }

            return new AuthResponseDto
            {
                IsSuccess = false,
                Errors = new List<string> { "Email hoặc mật khẩu không đúng." }
            };
        }

      
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress)
        {
            // Bước 1: Giải mã Access Token đã hết hạn để lấy Principal (người dùng)
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal?.Identity?.Name == null) // Kiểm tra User Name từ token
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Access Token không hợp lệ." } };
            }

            // Bước 2: Tìm người dùng dựa trên UserName từ Access Token
            var user = await _userManager.FindByNameAsync(principal.Identity.Name);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Người dùng không tồn tại." } };
            }

            // Bước 3: Tìm Refresh Token trong database của người dùng này
            var refreshToken = await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == user.Id);

            // Bước 4: Kiểm tra trạng thái của Refresh Token
            if (refreshToken == null)
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Refresh Token không tìm thấy." } };
            }
            if (refreshToken.Revoked != null) // Đã bị thu hồi (có thể là tấn công)
            {
                // Thu hồi tất cả các refresh token khác của user nếu phát hiện token bị thu hồi được sử dụng lại
                await RevokeAllUserRefreshTokens(user.Id, "Attempted reuse of a revoked token detected.");
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Refresh Token đã bị thu hồi." } };
            }
            if (refreshToken.Expires <= DateTime.UtcNow) // Đã hết hạn
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Refresh Token đã hết hạn." } };
            }


            // Bước 5: Thu hồi refresh token hiện tại và tạo token mới
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            // newAuthResponse.RefreshToken sẽ được gán cho replacedByToken sau khi tạo
            _dbContext.Update(refreshToken);
            await _dbContext.SaveChangesAsync();

            // Tạo Access Token và Refresh Token mới
            var newAuthResponse = await GenerateJwtToken(user, ipAddress);
                
            // Cập nhật token cũ với replacedByToken
            refreshToken.ReplacedByToken = newAuthResponse.RefreshToken;
            _dbContext.Update(refreshToken);
            await _dbContext.SaveChangesAsync();

            return newAuthResponse; // Trả về Access Token và Refresh Token mới
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null || refreshToken.Revoked != null || refreshToken.Expires <= DateTime.UtcNow)
            {
                // Token không tìm thấy, hoặc đã bị thu hồi, hoặc đã hết hạn
                return false;
            }

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReasonRevoked = "User initiated logout";

            _dbContext.Update(refreshToken);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<AuthResponseDto> AssignRoleToUserAsync(string email, string roleName)
        {
            if (!Enum.IsDefined(typeof(RolesEnum), roleName))
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { $"Vai trò '{roleName}' không hợp lệ." } };
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Người dùng không tồn tại." } };
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return new AuthResponseDto { IsSuccess = true, Errors = null };
            }
            return new AuthResponseDto { IsSuccess = false, Errors = result.Errors.Select(e => e.Description).ToList() };
        }

        public async Task<AuthResponseDto> RemoveRoleFromUserAsync(string email, string roleName)
        {
            if (!Enum.IsDefined(typeof(RolesEnum), roleName))
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { $"Vai trò '{roleName}' không hợp lệ." } };
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Người dùng không tồn tại." } };
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new List<string> { "Người dùng không có vai trò này." } };
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return new AuthResponseDto { IsSuccess = true, Errors = null };
            }
            return new AuthResponseDto { IsSuccess = false, Errors = result.Errors.Select(e => e.Description).ToList() };
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        private async Task<AuthResponseDto> GenerateJwtToken(ApplicationUser user, string ipAddress = null)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = Convert.ToDouble(jwtSettings["ExpiryMinutes"] ?? "15"); // Access Token ngắn hạn (15 phút)
            var refreshTokenExpiryDays = Convert.ToDouble(jwtSettings["RefreshTokenExpiryDays"] ?? "7"); // Refresh Token dài hạn (7 ngày)

            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName ?? ""),
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(expiryMinutes); // Thời gian hết hạn của Access Token

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            // Tạo Refresh Token
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"), // Chuỗi token ngẫu nhiên không có dấu gạch ngang
                Expires = DateTime.UtcNow.AddDays(refreshTokenExpiryDays), // Hết hạn sau 7 ngày
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserId = user.Id
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                Roles = userRoles.ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expires,
                RefreshToken = refreshToken.Token, // TRẢ VỀ REFRESH TOKEN
                IsSuccess = true,
                Errors = null
            };
        }

        // Phương thức trợ giúp để giải mã Access Token đã hết hạn
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // QUAN TRỌNG: Không xác thực thời gian sống khi giải mã token hết hạn
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration.GetSection("JwtSettings")["Issuer"],
                ValidAudience = _configuration.GetSection("JwtSettings")["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Validate token nhưng bỏ qua lỗi hết hạn
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                // Đảm bảo token là JWT và thuật toán ký là HMAC SHA256
                if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Access Token không hợp lệ.");

                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi giải mã Access Token hết hạn: {ex.Message}");
                return null;
            }
        }

        // Phương thức tùy chọn: Thu hồi tất cả refresh token của người dùng (ví dụ khi đổi mật khẩu, đăng nhập bất thường)
        private async Task RevokeAllUserRefreshTokens(string userId, string reason)
        {
            var refreshTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

            foreach (var token in refreshTokens)
            {
                token.Revoked = DateTime.UtcNow;
                token.ReasonRevoked = reason;
                _dbContext.Update(token);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}