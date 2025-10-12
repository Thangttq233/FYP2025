using FYP2025.Domain.Entities;
using FYP2025.Domain.Enums;
using System.Threading.Tasks;

namespace FYP2025.Domain.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(string id); 
        Task<IEnumerable<Category>> GetAllAsync();
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(string id); 
        Task<bool> ExistsAsync(string id);
        Task<IEnumerable<Category>> GetByMainCategoryAsync(MainCategoryType mainCategory);
    }
}
