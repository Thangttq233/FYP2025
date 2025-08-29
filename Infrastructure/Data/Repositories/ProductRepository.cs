// FYP2025/Infrastructure/Data/Repositories/ProductRepository.cs
using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FYP2025.Infrastructure.Data.Repositories
{
    // Kế thừa từ GenericRepository và triển khai IProductRepository
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        // Constructor cần nhận ApplicationDbContext
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        // GHI ĐÈ phương thức GetByIdAsync từ GenericRepository để bao gồm Variants
        // Phương thức này đã có trong GenericRepository, nên chỉ cần override
        public override async Task<Product> GetByIdAsync(string id)
        {
            return await _context.Products
                                 .Include(p => p.Variants)
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        // GHI ĐÈ phương thức GetAllAsync từ GenericRepository để bao gồm Variants
        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                                 .Include(p => p.Variants)
                                 .ToListAsync();
        }

        // TRIỂN KHAI phương thức GetProductsByCategoryIdAsync từ IProductRepository
        public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(string categoryId)
        {
            return await _context.Products
                                 .Where(p => p.CategoryId == categoryId)
                                 .Include(p => p.Variants)
                                 .ToListAsync();
        }

        // TRIỂN KHAI phương thức GetProductVariantByIdAsync từ IProductRepository
        public async Task<ProductVariant> GetProductVariantByIdAsync(string productVariantId)
        {
            return await _context.ProductVariants
                                 .Include(pv => pv.Product)
                                 .FirstOrDefaultAsync(pv => pv.Id == productVariantId);
        }

        // TRIỂN KHAI phương thức ExistsAsync từ IGenericRepository (Nếu bạn có khai báo ở đó)
        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }
    }
}