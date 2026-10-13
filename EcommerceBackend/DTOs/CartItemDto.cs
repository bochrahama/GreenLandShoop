using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.DTOs
{
    // DTO for adding an item to the cart 
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public Guid CartItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        [Range(1,int.MaxValue)]
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity; // Calculate total price based on quantity and price
    }
}
