using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using FYP2025.Infrastructure.Data; 
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FYP2025.Infrastructure.Data.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                                 .Where(o => o.UserId == userId)
                                 .Include(o => o.Items) 
                                 .OrderByDescending(o => o.OrderDate) // Sắp xếp theo ngày mới nhất
                                 .ToListAsync();
        }

        public async Task<Order> GetOrderDetailsAsync(string orderId)
        {
            return await _context.Orders
                                 .Include(o => o.Items) 
                                 .Include(o => o.User) // Bao gồm thông tin User (ApplicationUser)
                                 .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        // Ghi đè phương thức Generic để đảm bảo Include OrderItems và User khi lấy Order
        public override async Task<Order> GetByIdAsync(string id)
        {
            return await _context.Orders
                                 .Include(o => o.Items)
                                 .Include(o => o.User)
                                 .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}