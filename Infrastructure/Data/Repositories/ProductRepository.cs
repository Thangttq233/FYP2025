using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // For Guid

namespace FYP2025.Infrastructure.Data.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            // Bao gồm Category và Variants khi lấy Product
            return await _context.Products
                                 .Include(p => p.Category)
                                 .Include(p => p.Variants) // THÊM DÒNG NÀY
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            // Bao gồm Category và Variants khi lấy tất cả Products
            return await _context.Products
                                 .Include(p => p.Category)
                                 .Include(p => p.Variants) // THÊM DÒNG NÀY
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(string categoryId)
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .Include(p => p.Variants) // THÊM DÒNG NÀY
                                 .Where(p => p.CategoryId == categoryId)
                                 .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.Id))
            {
                product.Id = Guid.NewGuid().ToString();
            }

            // Gán Id cho các ProductVariant nếu chúng chưa có
            foreach (var variant in product.Variants)
            {
                if (string.IsNullOrEmpty(variant.Id))
                {
                    variant.Id = Guid.NewGuid().ToString();
                }
                variant.ProductId = product.Id; // Đảm bảo khóa ngoại được gán
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            // EF Core sẽ theo dõi các thay đổi của Product và các Variants được load.
            // Đối với các Variants bị thêm/xóa/sửa, EF Core sẽ xử lý nếu bạn đã tải chúng lên.
            // Để xử lý thêm/xóa variants một cách hiệu quả, có thể cần logic phức tạp hơn
            // hoặc các API riêng để quản lý variants.
            // Ở đây, chúng ta giả định rằng Variants được cập nhật cùng với Product chính.

            _context.Products.Update(product);

            // Dòng này rất quan trọng để đảm bảo các Variants cũng được cập nhật.
            // Nếu bạn gửi một Product với các Variants đã thay đổi (thêm mới, xóa bỏ, sửa đổi),
            // EF Core cần được thông báo về những thay đổi đó.
            // Cách đơn giản nhất là Load Product cũ, sau đó cập nhật thủ công các Variants.
            // Tuy nhiên, với Update(product), nếu product và variants của nó đã được tải,
            // EF Core sẽ theo dõi các thay đổi.
            // Nếu bạn muốn thay thế hoàn toàn danh sách variants, bạn cần:
            // 1. Tải Product hiện có từ DB cùng với các Variants của nó.
            // 2. Xóa các Variants cũ.
            // 3. Thêm các Variants mới từ 'product' được truyền vào.
            // Điều này làm phức tạp logic cập nhật. Để đơn giản, hãy giả định Variants
            // được quản lý qua một API riêng hoặc chỉ cập nhật các thuộc tính của Variant đã tồn tại.

            // Để update Variants hiệu quả hơn, bạn có thể tham khảo kỹ thuật "Disconnected entities"
            // hoặc sử dụng thư viện như Z.EntityFramework.Extensions.
            // Tạm thời, chúng ta sẽ dựa vào EF Core Change Tracker cơ bản.

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }
    }
}