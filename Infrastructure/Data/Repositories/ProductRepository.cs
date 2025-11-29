using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FYP2025.Infrastructure.Data.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string name, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products
                                .Include(p => p.Category)
                                .Include(p => p.Variants)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.Price >= minPrice.Value));
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.Price <= maxPrice.Value));
            }

            return await query.ToListAsync();
        }

        public override async Task<Product> GetByIdAsync(string id)
        {
            return await _context.Products
                                 .Include(p => p.Category) 
                                 .Include(p => p.Variants)
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                                 .Include(p => p.Category) 
                                 .Include(p => p.Variants)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(string categoryId)
        {
            return await _context.Products
                                 .Where(p => p.CategoryId == categoryId)
                                 .Include(p => p.Category) 
                                 .ToListAsync();
        }

        public async Task<ProductVariant> GetProductVariantByIdAsync(string productVariantId)
        {
            return await _context.ProductVariants
                                 .Include(pv => pv.Product)
                                 .FirstOrDefaultAsync(pv => pv.Id == productVariantId);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task<int> GetTotalStockQuantityAsync()
        {
            // EF Core sẽ dịch cái này thành lệnh SQL: SELECT SUM(StockQuantity) FROM ProductVariants
            return await _context.ProductVariants.SumAsync(v => v.StockQuantity);
        }
    }
}
