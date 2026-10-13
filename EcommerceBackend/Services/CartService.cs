using EcommerceBackend.Data;
using EcommerceBackend.DTOs;
using EcommerceBackend.Entities;
using EcommerceBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceBackend.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartEntityAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private static CartDto MapToDto(Cart cart)
        {
            return new CartDto
            {
                CartId = cart.CartId,
                Items = cart.Items?.Select(i => new CartItemDto
                {
                    ProductId = i.ProductId,
                    CartItemId = i.Id, 
                    ProductName = i.Product?.ProductName ?? string.Empty,
                    Price = i.Product?.ProductPrice ?? 0,
                    Quantity = i.Quantity
                }).ToList() ?? new List<CartItemDto>()
            };
        }

        public async Task<CartDto> GetCartAsync(string userId)
        {
            var cart = await GetOrCreateCartEntityAsync(userId);
            return MapToDto(cart);
        }

        public async Task<CartDto> AddItemAsync(string userId, CartItemDto cartItemDto)
        {

            var product = await _context.Products.FindAsync(cartItemDto.ProductId)
                ?? throw new InvalidOperationException("Product not found.");

    
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); 
            }

            var existingItem = cart.Items
                .FirstOrDefault(i => i.ProductId == cartItemDto.ProductId);

            if (existingItem is not null)
            {
                var newQuantity = existingItem.Quantity + cartItemDto.Quantity;
                if (newQuantity > product.ProductQuantity)
                    throw new InvalidOperationException("Not enough stock available.");

                existingItem.Quantity = newQuantity;
                _context.Entry(existingItem).State = EntityState.Modified; 
            }
            else
            {
                if (cartItemDto.Quantity > product.ProductQuantity)
                    throw new InvalidOperationException("Not enough stock available.");

                var newItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = cartItemDto.ProductId,
                    Quantity = cartItemDto.Quantity
                };
                _context.CartItems.Add(newItem); 
            }

            await _context.SaveChangesAsync();

           
            return await GetCartAsync(userId);
        }

        public async Task<bool> UpdateQuantityAsync(string userId, int productId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null) return false;

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem is null) return false;

            if (quantity <= 0)
            {
                _context.CartItems.Remove(existingItem);
            }
            else
            {
                existingItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveItemFromCartAsync(string userId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null) return false;

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem is null) return false;

            _context.CartItems.Remove(existingItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null || !cart.Items.Any()) return;

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
        }
    }
}
