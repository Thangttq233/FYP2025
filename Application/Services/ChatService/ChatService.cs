using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FYP2025.Application.Services.ChatService
{
    public class ChatService : IChatService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;

        public ChatService(IConversationRepository conversationRepository, IMessageRepository messageRepository)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId)
        {
            return await _messageRepository.GetMessagesByConversationIdAsync(conversationId);
        }

        public async Task<Conversation> GetOrCreateConversationAsync(string user1Id, string user2Id)
        {
            var conversation = await _conversationRepository.GetConversationByUserIdsAsync(user1Id, user2Id);

            if (conversation == null)
            {
                var newConversation = new Conversation
                {
                    User1Id = user1Id,
                    User2Id = user2Id
                };
                await _conversationRepository.AddAsync(newConversation);
                conversation = newConversation;
            }

            return conversation;
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(string userId)
        {
            return await _conversationRepository.GetConversationsByUserIdAsync(userId);
        }

        public async Task<Message> SaveMessageAsync(int conversationId, string senderId, string content)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(message);
            return message;
        }
    }
}
