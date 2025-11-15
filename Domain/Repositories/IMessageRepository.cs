using FYP2025.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FYP2025.Domain.Repositories
{
    public interface IMessageRepository
    {
        Task<Message> GetByIdAsync(int id);
        Task<IEnumerable<Message>> GetAllAsync();
        Task AddAsync(Message entity);
        Task UpdateAsync(Message entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Message>> GetMessagesByConversationIdAsync(int conversationId);
    }
}
