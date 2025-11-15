using FYP2025.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FYP2025.Domain.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation> GetByIdAsync(int id);
        Task<IEnumerable<Conversation>> GetAllAsync();
        Task AddAsync(Conversation entity);
        Task UpdateAsync(Conversation entity);
        Task DeleteAsync(int id);
        Task<Conversation> GetConversationByUserIdsAsync(string user1Id, string user2Id);
        Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(string userId);
    }
}
