using FYP2025.Application.DTOs;
using System.Threading.Tasks;

namespace FYP2025.Application.Services.Vnpay
{
    public interface IVnpayService
    {
        Task<string> CreateVnpayPaymentUrl(OrderDto order);
        Task<object> HandleVnpayUrl(string responseCode, string orderId);
    }
}
