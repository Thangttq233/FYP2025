using System.Collections.Generic;
using System.Threading.Tasks;
using FYP2025.Domain.Entities;

namespace FYP2025.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(string id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(string categoryId);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);

        // Có thể thêm các phương thức riêng cho variants nếu bạn muốn quản lý chúng độc lập
        // Task<ProductVariant?> GetProductVariantByIdAsync(string variantId);
        // Task AddProductVariantAsync(ProductVariant variant);
        // Task UpdateProductVariantAsync(ProductVariant variant);
        // Task DeleteProductVariantAsync(string variantId);
    }
}