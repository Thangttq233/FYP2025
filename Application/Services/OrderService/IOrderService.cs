using FYP2025.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FYP2025.Application.Services.OrderService
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderFromCartAsync(string userId, CreateOrderRequestDto request);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
        Task<OrderDto> GetOrderDetailsAsync(string orderId);
        Task<bool> UpdateOrderStatusAsync(string orderId, UpdateOrderStatusRequestDto request);
        Task<decimal> GetTotalRevenueAsync();
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<int> GetTotalOrdersAsync();
    }
}