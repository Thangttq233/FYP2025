using FYP2025.Configurations;
using FYP2025.Domain.Services.Cloudinary;
using FYP2025.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// Thêm các using cần thiết cho các lớp từ các project khác
using FYP2025.Domain.Repositories;
using FYP2025.Infrastructure.Data.Repositories;
using FYP2025.Application.Mappers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddControllers()
    .AddApplicationPart(Assembly.GetExecutingAssembly());


// Các cấu hình khác
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Upload 1 ảnh
app.MapPost("/upload-photo", async (IPhotoService photoService, HttpRequest request) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("Invalid form data");

    var form = await request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();

    if (file == null)
        return Results.BadRequest("No file uploaded");

    var result = await photoService.UploadPhotoAsync(file);

    if (result.Error != null)
        return Results.BadRequest(result.Error.Message);

    return Results.Ok(new { Url = result.SecureUrl.ToString() });
});

// Upload nhiều ảnh
app.MapPost("/upload-photos", async (IPhotoService photoService, HttpRequest request) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("Invalid form data");

    var form = await request.ReadFormAsync();
    var files = form.Files;

    if (files.Count == 0)
        return Results.BadRequest("No files uploaded");

    var urls = new List<string>();

    foreach (var file in files)
    {
        var result = await photoService.UploadPhotoAsync(file);

        if (result.Error != null)
            return Results.BadRequest(result.Error.Message);

        urls.Add(result.SecureUrl.ToString());
    }

    return Results.Ok(urls);
});

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
