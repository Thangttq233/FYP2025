using FYP2025.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FYP2025.Application.Services.ChatService
{
    public interface IChatService
    {
        Task<Conversation> GetOrCreateConversationAsync(string user1Id, string user2Id);
        Task<Message> SaveMessageAsync(int conversationId, string senderId, string content);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(string userId);
        Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId);
    }
}
