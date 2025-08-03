using FYP2025.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FYP2025.Domain.Repositories
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetCartByUserIdAsync(string userId);
        Task<CartItem> GetCartItemByIdAsync(string cartItemId); 
        Task<CartItem> GetCartItemByProductVariantIdAsync(string cartId, string productVariantId); 
        Task AddCartItemAsync(CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task DeleteCartItemAsync(string cartItemId);
        Task ClearCartAsync(string cartId); 
    }
}