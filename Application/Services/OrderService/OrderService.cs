using FYP2025.Application.DTOs;
using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using FYP2025.Infrastructure.Data; 
using AutoMapper;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore;
using FYP2025.Application.Services.Vnpay; 
using Microsoft.AspNetCore.Http;
using FYP2025.Application.Services.OrderService; 

namespace FYP2025.Application.Services.OrderServices 
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext; 
        private readonly IVnpayService _vnpayService;

        public OrderService(IOrderRepository orderRepository,
                            ICartRepository cartRepository,
                            IProductRepository productRepository,
                            UserManager<ApplicationUser> userManager,
                            IMapper mapper,
                            ApplicationDbContext dbContext,
                            IVnpayService vnpayService)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userManager = userManager;
            _mapper = mapper;
            _dbContext = dbContext;
            _vnpayService = vnpayService;
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

            // CHỈ KIỂM TRA TỒN KHO, KHÔNG GIẢM TỒN KHO Ở ĐÂY
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

                // TẠO OrderItem với snapshot data (để lưu giá tại thời điểm đặt hàng)
                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductVariantId = cartItem.ProductVariantId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = productVariant.Price,
                    ProductSnapshotName = productVariant.Product.Name,
                    ProductVariantSnapshotColor = productVariant.Color,
                    ProductVariantSnapshotSize = productVariant.Size,
                    ProductVariantSnapshotImageUrl = productVariant.ImageUrl
                });

                totalOrderPrice += cartItem.Quantity * productVariant.Price;
            }

            // Tạo Order chính với trạng thái Pending
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending, // Trạng thái ban đầu luôn là Pending
                TotalPrice = totalOrderPrice,
                ShippingAddress = request.ShippingAddress,
                PhoneNumber = request.PhoneNumber,
                CustomerName = user.FullName,
                Items = orderItems
            };

            await _orderRepository.AddAsync(order);

            // Xóa giỏ hàng sau khi tạo đơn hàng
            await _cartRepository.ClearCartAsync(cart.Id);

            var createdOrder = await _orderRepository.GetOrderDetailsAsync(order.Id);
            var orderDto = _mapper.Map<OrderDto>(createdOrder);
            return orderDto;
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _orderRepository.GetUserOrdersAsync(userId);
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

        // SỬA ĐỔI PHƯƠNG THỨC XỬ LÝ PHẢN HỒI TỪ VNPAY
        public async Task<bool> ProcessVnpayReturn(IQueryCollection vnpayData)
        {
            var secureHashIsValid = await _vnpayService.ProcessVnpayReturn(vnpayData);

            if (secureHashIsValid)
            {
                var vnp_ResponseCode = vnpayData["vnp_ResponseCode"].ToString();
                var vnp_TransactionStatus = vnpayData["vnp_TransactionStatus"].ToString();
                var orderId = vnpayData["vnp_TxnRef"].ToString();

                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    // Thanh toán thành công -> LÀ NƠI CHÚNG TA GIẢM TỒN KHO
                    var order = await _orderRepository.GetOrderDetailsAsync(orderId); // Lấy order với items
                    if (order != null && order.Status == OrderStatus.Pending) // Chỉ xử lý khi đơn hàng đang Pending
                    {
                        // Lặp qua từng item trong đơn hàng và giảm tồn kho
                        foreach (var orderItem in order.Items)
                        {
                            var productVariant = await _productRepository.GetProductVariantByIdAsync(orderItem.ProductVariantId);
                            if (productVariant != null)
                            {
                                productVariant.StockQuantity -= orderItem.Quantity;
                                _dbContext.ProductVariants.Update(productVariant);
                            }
                        }

                        // Cập nhật trạng thái đơn hàng
                        order.Status = OrderStatus.Processing;
                        await _orderRepository.UpdateAsync(order);
                        await _dbContext.SaveChangesAsync(); // Lưu tất cả thay đổi (cả tồn kho và trạng thái)
                        return true;
                    }
                }
            }

            // Nếu chữ ký không hợp lệ, hoặc thanh toán thất bại
            return false;
        }

        public async Task<string> CreateVnpayPaymentUrl(string userId, string orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                throw new ArgumentException("Đơn hàng không hợp lệ.");
            }

            if (order.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ có thể thanh toán các đơn hàng đang chờ xử lý.");
            }

            var paymentUrl = await _vnpayService.CreatePaymentUrl(order);

            return paymentUrl;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return orderDtos;
        }
    }
}