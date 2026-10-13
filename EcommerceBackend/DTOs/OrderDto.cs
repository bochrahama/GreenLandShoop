//the porpus of this file is to define a data transfer object (DTO) for an order in an e-commerce backend application. The OrderDto class contains properties that represent the essential details of an order, such as the order ID, order date, status, list of items, and shipping address. This DTO can be used to transfer order data between different layers of the application, such as from the backend to the frontend or between services.
namespace EcommerceBackend.DTOs
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
        public string ShippingAddress { get; set; } = string.Empty;
    }
}
