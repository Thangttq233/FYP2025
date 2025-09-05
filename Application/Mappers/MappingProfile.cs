// FYP2025.Application/Mappers/MappingProfile.cs
using AutoMapper;
using FYP2025.Application.DTOs;
using FYP2025.Domain.Entities;
using FYP2025.Infrastructure.Data;
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
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) 
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); 

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

            // Mapping cho User (ApplicationUser <-> UserDto)
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles sẽ được gán thủ công vì cần UserManager

            // Mappings cho Cart
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.TotalCartPrice, opt => opt.Ignore()); // Sẽ tính toán trong service
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductVariant.Product.Name))
                .ForMember(dest => dest.ProductVariantColor, opt => opt.MapFrom(src => src.ProductVariant.Color))
                .ForMember(dest => dest.ProductVariantSize, opt => opt.MapFrom(src => src.ProductVariant.Size))
                .ForMember(dest => dest.ProductVariantPrice, opt => opt.MapFrom(src => src.ProductVariant.Price))
                .ForMember(dest => dest.ProductVariantImageUrl, opt => opt.MapFrom(src => src.ProductVariant.ImageUrl));

            // CreateMap<AddToCartRequestDto, CartItem>(); // Sẽ map thủ công trong service để lấy ProductVariantId, Quantity
            // CreateMap<UpdateCartItemRequestDto, CartItem>(); // Sẽ map thủ công trong service để cập nhật Quantity

            // Mappings cho Order
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.FullName)) 
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email)); 
            CreateMap<OrderItem, OrderItemDto>();

            // CreateMap<CreateOrderRequestDto, Order>(); // Sẽ map thủ công trong service và lấy từ Cart/CartItems
            // CreateMap<UpdateOrderStatusRequestDto, Order>(); // Sẽ map thủ công trong service
        }
    }
}