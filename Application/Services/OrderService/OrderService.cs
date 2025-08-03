// FYP2025/Application/Services/Order/OrderService.cs
using FYP2025.Application.DTOs;
using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using FYP2025.Infrastructure.Data; // Cho ApplicationUser (khi lấy CustomerName/Email)
using AutoMapper;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Identity; // Cho UserManager để lấy thông tin user
using Microsoft.EntityFrameworkCore; // Cần cho UpdateAsync của Product

namespace FYP2025.Application.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext; // Cần thiết để cập nhật ProductVariant

        public OrderService(IOrderRepository orderRepository,
                            ICartRepository cartRepository,
                            IProductRepository productRepository,
                            UserManager<ApplicationUser> userManager,
                            IMapper mapper,
                            ApplicationDbContext dbContext)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userManager = userManager;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(string userId, CreateOrderRequestDto request)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any())
            {
                throw new InvalidOperationException("Giỏ hàng của bạn đang trống.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không hợp lệ.");
            }

            // Kiểm tra tồn kho và tính tổng giá
            var orderItems = new List<OrderItem>();
            decimal totalOrderPrice = 0;

            foreach (var cartItem in cart.Items)
            {
                var productVariant = await _productRepository.GetProductVariantByIdAsync(cartItem.ProductVariantId);
                if (productVariant == null || productVariant.Product == null)
                {
                    throw new InvalidOperationException($"Sản phẩm '{cartItem.ProductVariantId}' trong giỏ hàng không tồn tại hoặc đã bị xóa.");
                }
                if (productVariant.StockQuantity < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Không đủ tồn kho cho biến thể '{productVariant.Product.Name} - {productVariant.Color} - {productVariant.Size}'. Chỉ còn {productVariant.StockQuantity} sản phẩm.");
                }

                // Giảm số lượng tồn kho và lưu
                productVariant.StockQuantity -= cartItem.Quantity;
                _dbContext.ProductVariants.Update(productVariant); // Cập nhật trực tiếp ProductVariant
                // await _productRepository.UpdateAsync(productVariant.Product); // Không cần dòng này nữa vì đã cập nhật trực tiếp

                // Tạo OrderItem với snapshot data
                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductVariantId = cartItem.ProductVariantId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = productVariant.Price, // Giá tại thời điểm đặt hàng
                    ProductSnapshotName = productVariant.Product.Name,
                    ProductVariantSnapshotColor = productVariant.Color,
                    ProductVariantSnapshotSize = productVariant.Size,
                    ProductVariantSnapshotImageUrl = productVariant.ImageUrl
                });

                totalOrderPrice += cartItem.Quantity * productVariant.Price;
            }

            // Tạo Order chính
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending, // Trạng thái mặc định khi tạo đơn hàng
                TotalPrice = totalOrderPrice,
                ShippingAddress = request.ShippingAddress,
                PhoneNumber = request.PhoneNumber,
                CustomerName = user.FullName, // Lấy từ thông tin user
                Items = orderItems
            };

            await _orderRepository.AddAsync(order);

            // Xóa giỏ hàng sau khi tạo đơn hàng
            await _cartRepository.ClearCartAsync(cart.Id);

            // Lấy lại order với thông tin User đầy đủ để mapping DTO
            var createdOrder = await _orderRepository.GetOrderDetailsAsync(order.Id);
            var orderDto = _mapper.Map<OrderDto>(createdOrder);
            return orderDto;
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _orderRepository.GetUserOrdersAsync(userId);
            // Để map CustomerName và CustomerEmail, cần Include User trong Repository GetUserOrdersAsync
            // Hoặc lặp qua từng order và lấy user từ UserManager nếu cần

            // Nếu OrderRepository.GetUserOrdersAsync đã include User, chỉ cần map
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return orderDtos;
        }

        public async Task<OrderDto> GetOrderDetailsAsync(string orderId)
        {
            var order = await _orderRepository.GetOrderDetailsAsync(orderId);
            if (order == null) return null;

            var orderDto = _mapper.Map<OrderDto>(order);
            return orderDto;
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, UpdateOrderStatusRequestDto request)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            order.Status = request.NewStatus;
            await _orderRepository.UpdateAsync(order);
            return true;
        }
    }
}