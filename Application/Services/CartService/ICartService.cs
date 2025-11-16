using FYP2025.Application.DTOs;
using System.Threading.Tasks;

namespace FYP2025.Application.Services.CartService
{
    public interface ICartService
    {
        Task<CartDto> GetUserCartAsync(string userId);
        Task<CartDto> AddItemToCartAsync(string userId, AddToCartRequestDto request);
        Task<CartDto> UpdateCartItemQuantityAsync(string userId, UpdateCartItemRequestDto request);
        Task<CartDto> RemoveCartItemAsync(string userId, string cartItemId);
        Task<bool> ClearUserCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
    }
}