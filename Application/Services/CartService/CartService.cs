// FYP2025/Application/Services/Cart/CartService.cs
using FYP2025.Application.DTOs;
using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using AutoMapper;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using FYP2025.Domain.Entities;

namespace FYP2025.Application.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository; 
        private readonly IMapper _mapper;

        public CartService(ICartRepository cartRepository,
                           IProductRepository productRepository,
                           IMapper mapper)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

       

        private async Task<Cart> GetOrCreateUserCartAsync(string userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new FYP2025.Domain.Entities.Cart { Id = Guid.NewGuid().ToString(), UserId = userId };
                await _cartRepository.AddAsync(cart);
            }
            return cart;
        }

        public async Task<CartDto> GetUserCartAsync(string userId)
        {
            var cart = await GetOrCreateUserCartAsync(userId);
            var cartDto = _mapper.Map<CartDto>(cart);
            if (cart.Items != null)
            {
                cartDto.TotalCartPrice = cart.Items.Sum(item => item.Quantity * item.ProductVariant.Price);
            }
            return cartDto;
        }

        public async Task<CartDto> AddItemToCartAsync(string userId, AddToCartRequestDto request)
        {
            var cart = await GetOrCreateUserCartAsync(userId);

            var productVariant = await _productRepository.GetProductVariantByIdAsync(request.ProductVariantId);
            if (productVariant == null)
            {
                throw new ArgumentException($"Product variant with ID {request.ProductVariantId} not found.");
            }
            if (productVariant.StockQuantity < request.Quantity)
            {
                throw new ArgumentException($"Not enough stock for variant {productVariant.Color} - {productVariant.Size}. Available: {productVariant.StockQuantity}");
            }

            var existingItem = await _cartRepository.GetCartItemByProductVariantIdAsync(cart.Id, request.ProductVariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                await _cartRepository.UpdateCartItemAsync(existingItem);
            }
            else
            {
                var newCartItem = new CartItem
                {
                    Id = Guid.NewGuid().ToString(),
                    CartId = cart.Id,
                    ProductVariantId = request.ProductVariantId,
                    Quantity = request.Quantity
                };
                await _cartRepository.AddCartItemAsync(newCartItem);
            }
            var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
            var updatedCartDto = _mapper.Map<CartDto>(updatedCart);
            if (updatedCart.Items != null)
            {
                updatedCartDto.TotalCartPrice = updatedCart.Items.Sum(item => item.Quantity * item.ProductVariant.Price);
            }
            return updatedCartDto;
        }

        public async Task<CartDto> UpdateCartItemQuantityAsync(string userId, UpdateCartItemRequestDto request)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) throw new InvalidOperationException("Cart not found for user.");

            var existingItem = cart.Items.FirstOrDefault(item => item.Id == request.CartItemId);
            if (existingItem == null)
            {
                throw new ArgumentException($"Cart item with ID {request.CartItemId} not found in cart.");
            }
            var productVariant = await _productRepository.GetProductVariantByIdAsync(existingItem.ProductVariantId);
            if (productVariant == null)
            {
                throw new InvalidOperationException("Associated product variant not found. Please remove this item from cart.");
            }
            if (productVariant.StockQuantity < request.Quantity)
            {
                throw new ArgumentException($"Not enough stock for variant {productVariant.Color} - {productVariant.Size}. Available: {productVariant.StockQuantity}");
            }

            if (request.Quantity <= 0)
            {
                await _cartRepository.DeleteCartItemAsync(request.CartItemId);
            }
            else
            {
                existingItem.Quantity = request.Quantity;
                await _cartRepository.UpdateCartItemAsync(existingItem);
            }

            var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
            var updatedCartDto = _mapper.Map<CartDto>(updatedCart);
            if (updatedCart.Items != null)
            {
                updatedCartDto.TotalCartPrice = updatedCart.Items.Sum(item => item.Quantity * item.ProductVariant.Price);
            }
            return updatedCartDto;
        }

        public async Task<CartDto> RemoveCartItemAsync(string userId, string cartItemId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) throw new InvalidOperationException("Cart not found for user.");

            var existingItem = cart.Items.FirstOrDefault(item => item.Id == cartItemId);
            if (existingItem == null)
            {
                throw new ArgumentException($"Cart item with ID {cartItemId} not found in cart.");
            }

            await _cartRepository.DeleteCartItemAsync(cartItemId);
            var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
            var updatedCartDto = _mapper.Map<CartDto>(updatedCart);
            if (updatedCart.Items != null)
            {
                updatedCartDto.TotalCartPrice = updatedCart.Items.Sum(item => item.Quantity * item.ProductVariant.Price);
            }
            return updatedCartDto;
        }

        public async Task<bool> ClearUserCartAsync(string userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return true; 

            await _cartRepository.ClearCartAsync(cart.Id);
            return true;
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return 0;

            return cart.Items.Sum(i => i.Quantity);
        }

    }
}