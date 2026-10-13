namespace EcommerceBackend.DTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalPrice => Items.Sum(item => item.TotalPrice); // Calculate total price of all items in the cart
    }
}
