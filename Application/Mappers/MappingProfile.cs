// FYP2025.Application/Mappers/MappingProfile.cs
using AutoMapper;
using FYP2025.Application.DTOs;
using FYP2025.Domain.Entities;
using System.Linq;

namespace FYP2025.Application.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category Mappings (Không đổi)
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            // ProductVariant Mappings
            CreateMap<ProductVariant, ProductVariantDto>().ReverseMap();

            // CreateProductVariantDto => ProductVariant
            CreateMap<CreateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID sẽ được tạo tự động
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) // ProductId được gán trong controller
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); // <--- ImageUrl sẽ được gán thủ công từ IFormFile

            // UpdateProductVariantDto => ProductVariant
            CreateMap<UpdateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); // ImageUrl sẽ được cập nhật qua API riêng cho Variant

            // Product Mappings (Từ Entity sang DTO)
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            // CreateProductDto => Product
            // QUAN TRỌNG:
            // 1. Bỏ qua ImageUrl vì nó sẽ được gán thủ công từ ProductImageFile trong Controller.
            // 2. Variants sẽ được AutoMapper ánh xạ tự động nếu tên thuộc tính và kiểu khớp.
            //    Chúng ta sẽ lặp qua product.Variants trong controller để xử lý ImageFile và gán ID.
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // <--- Bỏ qua ImageUrl
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants)); // <--- AutoMapper sẽ cố gắng map Variants

            // UpdateProductDto => Product
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // Có API riêng để cập nhật ảnh Product chính
                .ForMember(dest => dest.Variants, opt => opt.Ignore()); // Variants có API riêng để quản lý
        }
    }
}