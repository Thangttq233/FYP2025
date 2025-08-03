using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FYP2025.Infrastructure.Data.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return await _context.Carts
                                 .Include(c => c.Items) // Bao gồm CartItems
                                     .ThenInclude(ci => ci.ProductVariant) // Bao gồm ProductVariant
                                         .ThenInclude(pv => pv.Product) // Bao gồm Product
                                 .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<CartItem> GetCartItemByIdAsync(string cartItemId)
        {
            return await _context.CartItems
                                 .Include(ci => ci.ProductVariant)
                                     .ThenInclude(pv => pv.Product)
                                 .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
        }

        public async Task<CartItem> GetCartItemByProductVariantIdAsync(string cartId, string productVariantId)
        {
            return await _context.CartItems
                                 .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductVariantId == productVariantId);
        }

        public async Task AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCartItemAsync(string cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string cartId)
        {
            var cartItems = await _context.CartItems.Where(ci => ci.CartId == cartId).ToListAsync();
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public override async Task<Cart> GetByIdAsync(string id)
        {
            return await _context.Carts
                                 .Include(c => c.Items)
                                     .ThenInclude(ci => ci.ProductVariant)
                                         .ThenInclude(pv => pv.Product)
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}