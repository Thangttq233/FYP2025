using FYP2025.Configurations;
using FYP2025.Domain.Services.Cloudinary;
using FYP2025.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// Thêm các using cần thiết cho các lớp từ các project khác
using FYP2025.Domain.Repositories;
using FYP2025.Infrastructure.Data.Repositories;
using FYP2025.Application.Mappers;
using System.Reflection;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.IdentityModel.Tokens; 
using System.Text;
using FYP2025.Application.Services.Auth; 
using Npgsql;
using FYP2025.Application.Services.CartService;
using FYP2025.Application.Services.OrderService;
using Microsoft.OpenApi.Models; 

var builder = WebApplication.CreateBuilder(args);

//  LỖI DATETIME.KIND=UNSPECIFIED 

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);



// Đăng ký DbContext với PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình CloudinarySettings bind từ appsettings.json
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Đăng ký PhotoService vào DI container
builder.Services.AddScoped<IPhotoService, PhotoService>();

// Đăng ký Repositories 
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Đăng ký AuthService 
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Add AutoMapper 
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddControllers()
    .AddApplicationPart(Assembly.GetExecutingAssembly());

// Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); 


// CẤU HÌNH JWT AUTHENTICATION 

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
{
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
        ValidateIssuer = true, 
        ValidateAudience = true, 
        ValidateLifetime = true, 
        ValidateIssuerSigningKey = true, 
        ValidIssuer = issuer, 
        ValidAudience = audience, 
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) 
    };
});

builder.Services.AddAuthorization(); 

builder.Services.AddSwaggerGen(options =>
{
    // Cấu hình để thêm nút "Authorize" vào Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Vui lòng nhập 'Bearer ' theo sau là token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); 

app.UseAuthentication(); 
app.UseAuthorization();  
app.MapControllers();
app.Run();