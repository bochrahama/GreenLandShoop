using EcommerceBackend.DTOs;
namespace EcommerceBackend.Services
{
    //it's for the cart service to manage the cart items and the cart itself
    public interface ICartService
    {
        
        Task<CartDto> GetCartAsync(string userId);
        Task<CartDto> AddItemAsync(string userId, CartItemDto cartItemDto);
        Task<bool> UpdateQuantityAsync(string userId, int CartItemId , int Quantity);
        Task<bool> RemoveItemFromCartAsync(string userId, int productId);
        Task ClearCartAsync(string userId);

    }
}
