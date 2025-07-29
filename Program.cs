using FYP2025.Configurations;
using FYP2025.Domain.Services.Cloudinary;
using FYP2025.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// Thêm các using cần thiết cho các lớp từ các project khác
using FYP2025.Domain.Repositories;
using FYP2025.Infrastructure.Data.Repositories;
using FYP2025.Application.Mappers;
using System.Reflection;
using Microsoft.AspNetCore.Identity; // Cho Identity
using Microsoft.AspNetCore.Authentication.JwtBearer; // Cho JWT
using Microsoft.IdentityModel.Tokens; // Cho TokenValidationParameters
using System.Text;
using FYP2025.Application.Services.Auth; // Cho AuthService (đảm bảo namespace này đúng)
using Npgsql; // <--- THÊM USING NÀY CHO NPGSQL

var builder = WebApplication.CreateBuilder(args);

// --- GIẢI PHÁP CHO LỖI DATETIME.KIND=UNSPECIFIED ---
// Đặt dòng này TRƯỚC AddDbContext
// Nó cho phép Npgsql xử lý DateTime với Kind=Unspecified bằng cách coi chúng là Local time.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// --- KẾT THÚC GIẢI PHÁP DATETIME.KIND ---


// Đăng ký DbContext với PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình CloudinarySettings bind từ appsettings.json
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Đăng ký PhotoService vào DI container
builder.Services.AddScoped<IPhotoService, PhotoService>();

// Đăng ký Repositories (CHỈ MỘT LẦN)
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Đăng ký AuthService (CHỈ MỘT LẦN)
builder.Services.AddScoped<IAuthService, AuthService>();

// Add AutoMapper (CHỈ MỘT LẦN)
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddControllers()
    .AddApplicationPart(Assembly.GetExecutingAssembly());

// Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Cần thiết cho các tính năng như reset mật khẩu


// --- BẮT ĐẦU CẤU HÌNH JWT AUTHENTICATION ---

// Lấy Secret Key từ cấu hình (ví dụ: appsettings.json)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
{
    // Đây là một kiểm tra an toàn, bạn nên có các giá trị này trong appsettings.json
    throw new InvalidOperationException("JwtSettings: Secret, Issuer, or Audience is not configured. Please check appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Xác thực nhà phát hành token
        ValidateAudience = true, // Xác thực đối tượng token
        ValidateLifetime = true, // Xác thực thời gian sống của token
        ValidateIssuerSigningKey = true, // Xác thực khóa ký token
        ValidIssuer = issuer, // Nhà phát hành hợp lệ
        ValidAudience = audience, // Đối tượng hợp lệ
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) // Khóa ký
    };
});

builder.Services.AddAuthorization(); // <--- Đảm bảo dòng này có để kích hoạt phân quyền

// --- KẾT THÚC CẤU HÌNH JWT AUTHENTICATION ---


// Các cấu hình khác
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // <--- Giữ lại dòng này ở đây, không trùng lặp

// --- THÊM MIDDLEWARE XÁC THỰC VÀ PHÂN QUYỀN VÀO PIPELINE ---
app.UseAuthentication(); // <--- PHẢI ĐẶT TRƯỚC UseAuthorization
app.UseAuthorization();  // <--- THÊM DÒNG NÀY VÀO ĐÂY
// --- KẾT THÚC THÊM MIDDLEWARE ---

app.MapControllers();

app.Run();