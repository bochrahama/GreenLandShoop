using EcommerceBackend.DTOs;
using EcommerceBackend.Data;
using EcommerceBackend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Forms;
namespace EcommerceBackend.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        // the porpes is to create an order from the user's cart, deduct the product quantities,
        // and save the order to the database
        // .it works by first retrieving the user's cart from the database,
        // then creating a new order and adding the items from the cart to the order.
        // It also checks if there is enough stock for each product in the cart before deducting the quantities.
        // Finally, it saves the order to the database and returns an OrderDto object representing the newly created order.
        public async Task<OrderDto> CheckoutAsync(string userId, CheckoutDto checkoutDto)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null || !cart.Items.Any())
            {
                throw new InvalidOperationException("Cart is empty or does not exist.");
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oder = new Order
                {
                    UserId = userId,
                    ShippingAddress = checkoutDto.ShippingAddress,
                    Status = OrderStatus.Pending,
                };

                 decimal totalAmount = 0;
                foreach (var item in cart.Items)
                {
                    var product = item.Product!;
                    if (product.ProductQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException("Insufficient stock for product.");
                    }
                    product.ProductQuantity -= item.Quantity;
                    var orderItem = new OrderItem
                    {
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.ProductPrice
                    };
                    oder.Items.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }
                _context.Orders.Add(oder);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return MapToDto(oder);
             }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                throw new InvalidOperationException(
                    "Stock changed while processing your order. Please try again."
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        //Get all orders for a user
        //it works by querying the database for all orders associated with the specified user ID, including their related order items.
        //The retrieved orders are then mapped to OrderDto objects and returned as an IEnumerable<OrderDto>.
        public async Task<IEnumerable<OrderDto>> GetOrderForUserAsync(string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return orders.Select(MapToDto);
        }
        //it works by querying the databasefor a specific order associated
        //with the specified user ID and order ID, including its related order items.
        public async Task<OrderDto?> GetOrderByIdAsync(string userId, Guid orderId)
        {
           var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Id == orderId);
          return order != null ? MapToDto(order) : null;
        }
        private static OrderDto MapToDto(Order order)
        => new OrderDto
        {
            OrderId = order.Id,
            OrderDate = order.CreatedAt.DateTime,
            Status = order.Status.ToString(),
            ShippingAddress = order.ShippingAddress,
            Items = order.Items.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product!.ProductName,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList(),
            
        };
    }
}
