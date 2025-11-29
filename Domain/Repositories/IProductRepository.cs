using System.Collections.Generic;
using System.Threading.Tasks;
using FYP2025.Domain.Entities;

namespace FYP2025.Domain.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(string categoryId);
        Task<ProductVariant> GetProductVariantByIdAsync(string productVariantId);
        Task<IEnumerable<Product>> SearchProductsAsync(string name, decimal? minPrice, decimal? maxPrice);
        Task<int> GetTotalStockQuantityAsync();
    }
}