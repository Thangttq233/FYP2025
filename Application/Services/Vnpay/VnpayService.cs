using FYP2025.Configurations;
using FYP2025.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;
using FYP2025.Domain.Repositories;
using FYP2025.Application.DTOs;
using FYP2025.Infrastructure.Data;

namespace FYP2025.Application.Services.Vnpay
{
    public class VnpayService : IVnpayService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly VnpaySettings _vnpaySettings;
        private readonly IProductRepository _productRepository; 
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VnpayService(
            IOrderRepository orderRepository,
            IOptions<VnpaySettings> vnpaySettings,
            IProductRepository productRepository, 
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _vnpaySettings = vnpaySettings.Value;
            _productRepository = productRepository; 
            _dbContext = dbContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private SortedDictionary<string, string> SortObject(Dictionary<string, string> obj)
        {
            var sorted = new SortedDictionary<string, string>(StringComparer.Ordinal);

            foreach (var kv in obj)
            {
                var encodedKey = Uri.EscapeDataString(kv.Key);
                var encodedValue = Uri.EscapeDataString(kv.Value).Replace("%20", "+");

                sorted[encodedKey] = encodedValue;
            }

            return sorted;
        }

        public async Task<string> CreateVnpayPaymentUrl(OrderDto order)
        {
            var ipAddr = "127.0.0.1";
            var tmnCode = "Z9IS49CN";
            var secretKey = "27PTM3F7M727KRW3IJ1ZKP6LYJ6EZ4TT";
            var vnpUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var returnUrl = "http://localhost:5173/order-success";
            var amount = order.TotalPrice;
            var orderId = order.Id;
            const int exchangeRate = 25000;
            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var rawParams = new Dictionary<string, string>
    {
        { "vnp_Version", "2.1.0" },
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", tmnCode },
        { "vnp_Locale", "vn" },
        { "vnp_CurrCode", "VND" },
        { "vnp_TxnRef", orderId },
        { "vnp_OrderInfo", $"Thanh toán đơn hàng {orderId}" },
        { "vnp_OrderType", "other" },
        { "vnp_Amount", (amount * 100).ToString() },
        { "vnp_IpAddr", ipAddr },
        { "vnp_CreateDate", createDate },
        { "vnp_BankCode", "NCB" },
        { "vnp_ReturnUrl", returnUrl }
    };
            var sortedParams = SortObject(rawParams);
            var signData = string.Join("&",
                sortedParams.Select(p => $"{p.Key}={p.Value}")
            );
            string secureHash;
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
                secureHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            sortedParams.Add("vnp_SecureHash", secureHash);
            var finalQuery = string.Join("&",
                sortedParams.Select(p => $"{p.Key}={p.Value}")
            );

            var paymentUrl = $"{vnpUrl}?{finalQuery}";

            return paymentUrl;
        }


        public async Task<object> HandleVnpayUrl(string responseCode, string orderId)
        {
            if (responseCode == "00") 
            {
                try
                {
                    var order = await _orderRepository.GetOrderDetailsAsync(orderId);
                    if (order == null)
                    {
                        throw new Exception("Order not found.");
                    }
                    if (order.PaymentStatus == PaymentStatus.Unpaid)
                    {
                        order.PaymentStatus = PaymentStatus.Paid;
                        order.Status = OrderStatus.Processing; 
                        await _orderRepository.UpdateAsync(order); 
                        foreach (var orderItem in order.Items)
                        {
                            var productVariant = await _productRepository.GetProductVariantByIdAsync(orderItem.ProductVariantId);
                            if (productVariant != null)
                            {
                                if (productVariant.StockQuantity < orderItem.Quantity)
                                {
                                    throw new Exception($"Out of stock for variant {productVariant.Id} while confirming payment.");
                                }
                                productVariant.StockQuantity -= orderItem.Quantity;
                                _dbContext.ProductVariants.Update(productVariant);
                            }
                        }
                        await _dbContext.SaveChangesAsync();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error updating order payment status and stock: " + ex.Message);
                }
            }
            else 
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order != null && order.PaymentStatus == PaymentStatus.Unpaid)
                {
                    order.Status = OrderStatus.Cancelled; 
                    await _orderRepository.UpdateAsync(order);
                }
                return true; 
            }
        }
    }

}

