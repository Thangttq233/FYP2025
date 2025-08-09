using FYP2025.Domain.Entities;
using System.Threading.Tasks;

namespace FYP2025.Application.Services.Vnpay
{
    public interface IVnpayService
    {
        Task<string> CreatePaymentUrl(Order order);
        Task<bool> ProcessVnpayReturn(IQueryCollection vnpayData);
    }
}