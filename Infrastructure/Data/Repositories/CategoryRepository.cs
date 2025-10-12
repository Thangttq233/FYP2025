using FYP2025.Domain.Entities;
using FYP2025.Domain.Enums;
using FYP2025.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FYP2025.Infrastructure.Data.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(string id) 
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            // Gán GUID mới nếu Id rỗng hoặc null (chỉ khi Id không được cung cấp từ bên ngoài)
            if (string.IsNullOrEmpty(category.Id))
            {
                category.Id = Guid.NewGuid().ToString();
            }
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id) 
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id) 
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetByMainCategoryAsync(MainCategoryType mainCategory)
        {
            return await _context.Categories
                .Where(c => c.MainCategory == mainCategory)
                .ToListAsync();
        }
    }
}
