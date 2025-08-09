// FYP2025/Application/Services/Vnpay/VnpayService.cs
using FYP2025.Application.Services.Vnpay;
using FYP2025.Configurations;
using FYP2025.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web; // Thêm using này

namespace FYP2025.Application.Services.Vnpay
{
    public class VnpayService : IVnpayService
    {
        private readonly VnpaySettings _vnpaySettings;
        private readonly IConfiguration _configuration;

        public VnpayService(IOptions<VnpaySettings> vnpaySettings, IConfiguration configuration)
        {
            _vnpaySettings = vnpaySettings.Value;
            _configuration = configuration;
        }

        public async Task<string> CreatePaymentUrl(Order order)
        {
            var vnp_Url = _vnpaySettings.BaseUrl;
            var vnp_TmnCode = _vnpaySettings.TmnCode;
            var vnp_HashSecret = _vnpaySettings.HashSecret;

            var vnpayData = new SortedDictionary<string, string>();
            vnpayData.Add("vnp_Version", "2.1.0");
            vnpayData.Add("vnp_Command", "pay");
            vnpayData.Add("vnp_TmnCode", vnp_TmnCode);
            vnpayData.Add("vnp_Amount", (order.TotalPrice * 100).ToString()); // Số tiền phải nhân 100
            vnpayData.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpayData.Add("vnp_CurrCode", "VND");
            vnpayData.Add("vnp_IpAddr", "127.0.0.1"); // IP Address của client
            vnpayData.Add("vnp_Locale", "vn");
            vnpayData.Add("vnp_OrderInfo", $"Thanh toan don hang {order.Id}");
            vnpayData.Add("vnp_OrderType", "other");
            vnpayData.Add("vnp_ReturnUrl", _vnpaySettings.ReturnUrl);
            vnpayData.Add("vnp_TxnRef", order.Id); // Mã giao dịch của bạn

            var hashData = new StringBuilder();
            foreach (var (key, value) in vnpayData)
            {
                hashData.Append(key + "=" + value);
                hashData.Append("&");
            }

            hashData.Remove(hashData.Length - 1, 1);
            var secureHash = HmacSha512(vnp_HashSecret, hashData.ToString());

            var paymentUrl = $"{vnp_Url}?" + string.Join("&", vnpayData.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}")) + $"&vnp_SecureHash={secureHash}";

            return paymentUrl;
        }

        public async Task<bool> ProcessVnpayReturn(IQueryCollection vnpayData)
        {
            // Lấy các tham số từ query string
            var secureHash = vnpayData["vnp_SecureHash"];

            // Tạo chuỗi để xác minh chữ ký
            var sortedVnpayData = new SortedDictionary<string, string>(
                vnpayData.ToDictionary(k => k.Key, v => v.Value.ToString()));

            // SỬA LỖI: LOẠI BỎ vnp_SecureHash TỪ DICTIONARY ĐÃ TẠO
            if (sortedVnpayData.ContainsKey("vnp_SecureHash"))
            {
                sortedVnpayData.Remove("vnp_SecureHash");
            }

            var hashData = new StringBuilder();
            foreach (var (key, value) in sortedVnpayData)
            {
                hashData.Append(key + "=" + value);
                hashData.Append("&");
            }
            hashData.Remove(hashData.Length - 1, 1);

            // Xác minh chữ ký
            var vnp_HashSecret = _vnpaySettings.HashSecret;
            var mySecureHash = HmacSha512(vnp_HashSecret, hashData.ToString());

            if (mySecureHash != secureHash)
            {
                return false; // Chữ ký không hợp lệ, trả về false
            }

            // Chữ ký hợp lệ, trả về true
            return true;
        }
        private string HmacSha512(string key, string data)
        {
            var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}