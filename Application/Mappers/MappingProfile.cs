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

            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();


            CreateMap<ProductVariant, ProductVariantDto>().ReverseMap();

   
            CreateMap<CreateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) 
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); 

 
            CreateMap<UpdateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); 


            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));


            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) 
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants)); 


            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) 
                .ForMember(dest => dest.Variants, opt => opt.Ignore()); 

   
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); 

   
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.TotalCartPrice, opt => opt.Ignore()); 
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductVariant.Product.Name))
                .ForMember(dest => dest.ProductVariantColor, opt => opt.MapFrom(src => src.ProductVariant.Color))
                .ForMember(dest => dest.ProductVariantSize, opt => opt.MapFrom(src => src.ProductVariant.Size))
                .ForMember(dest => dest.ProductVariantPrice, opt => opt.MapFrom(src => src.ProductVariant.Price))
                .ForMember(dest => dest.ProductVariantImageUrl, opt => opt.MapFrom(src => src.ProductVariant.ImageUrl));

         


            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User.FullName)) 
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email)); 
            CreateMap<OrderItem, OrderItemDto>();


        }
    }
}