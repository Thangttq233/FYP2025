using FYP2025.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FYP2025.Domain.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order> GetOrderDetailsAsync(string orderId); 
    }
}