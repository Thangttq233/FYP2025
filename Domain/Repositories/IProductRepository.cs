// FYP2025/Domain/Repositories/IProductRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using FYP2025.Domain.Entities;

namespace FYP2025.Domain.Repositories
{
    // IProductRepository kế thừa từ IGenericRepository
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(string categoryId);
        Task<ProductVariant> GetProductVariantByIdAsync(string productVariantId);
    }
}