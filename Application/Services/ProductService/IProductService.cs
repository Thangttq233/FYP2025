
using FYP2025.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FYP2025.Application.Services.ProductService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProductsAsync();
        Task<ProductDto> GetProductByIdAsync(string id);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(string categoryId);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto> UpdateProductAsync(string id, UpdateProductDto updateProductDto);
        Task UpdateProductImageAsync(string id, IFormFile imageFile);
        Task DeleteProductAsync(string id);
        Task<IEnumerable<ProductVariantDto>> GetProductVariantsAsync(string productId);
        Task<ProductVariantDto> GetProductVariantAsync(string productId, string variantId);
        Task<ProductVariantDto> AddProductVariantAsync(string productId, CreateProductVariantDto createVariantDto);
        Task UpdateProductVariantAsync(string productId, string variantId, UpdateProductVariantDto updateVariantDto);
        Task DeleteProductVariantAsync(string productId, string variantId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string name, decimal? minPrice, decimal? maxPrice);
        Task<int> GetTotalStockQuantityAsync();
    }
}
