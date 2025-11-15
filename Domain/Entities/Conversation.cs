using FYP2025.Infrastructure.Data;

namespace FYP2025.Domain.Entities
{
    public class Conversation
    {
        public int Id { get; set; }
        public string User1Id { get; set; }
        public ApplicationUser User1 { get; set; }
        public string User2Id { get; set; }
        public ApplicationUser User2 { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}