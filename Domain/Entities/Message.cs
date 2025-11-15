using FYP2025.Infrastructure.Data;

namespace FYP2025.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public string? SenderId { get; set; }
        public ApplicationUser Sender { get; set; }
        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }
    }
}