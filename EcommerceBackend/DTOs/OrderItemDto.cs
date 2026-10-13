//the porpus of this file is to define a data transfer object (DTO) for an order item in an e-commerce backend application. The OrderItemDto class contains properties that represent the essential details of an order item, such as the product ID, product name, unit price, quantity, and total price. This DTO can be used to transfer order item data between different layers of the application, such as from the backend to the frontend or between services.
namespace EcommerceBackend.DTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity; // Calculate total price based on quantity and price
    }
}
