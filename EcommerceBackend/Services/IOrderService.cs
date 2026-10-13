using EcommerceBackend.DTOs;
namespace EcommerceBackend.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CheckoutAsync(string userId, CheckoutDto checkoutDto);
        Task<IEnumerable<OrderDto>> GetOrderForUserAsync(string userId);
        Task<OrderDto?> GetOrderByIdAsync(string userId, Guid orderId);
    }
}
