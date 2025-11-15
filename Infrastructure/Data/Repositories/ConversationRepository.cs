using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using FYP2025.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FYP2025.Infrastructure.Data.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ApplicationDbContext _context;

        public ConversationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Conversation entity)
        {
            await _context.Conversations.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var conversation = await GetByIdAsync(id);
            if (conversation != null)
            {
                _context.Conversations.Remove(conversation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Conversation>> GetAllAsync()
        {
            return await _context.Conversations.ToListAsync();
        }

        public async Task<Conversation> GetByIdAsync(int id)
        {
            return await _context.Conversations.FindAsync(id);
        }

        public async Task<Conversation> GetConversationByUserIdsAsync(string user1Id, string user2Id)
        {
            return await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => 
                    (c.User1Id == user1Id && c.User2Id == user2Id) || 
                    (c.User1Id == user2Id && c.User2Id == user1Id));
        }

        public async Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(string userId)
        {
            return await _context.Conversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Include(c => c.Messages.OrderByDescending(m => m.Timestamp).Take(1)) // Get latest message for preview
                .Include(c => c.User1)
                .Include(c => c.User2)
                .OrderByDescending(c => c.Messages.Max(m => m.Timestamp))
                .ToListAsync();
        }

        public async Task UpdateAsync(Conversation entity)
        {
            _context.Conversations.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
