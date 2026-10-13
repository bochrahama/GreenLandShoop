using System;

namespace EcommerceBackend.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Order? Order { get; set; } // Navigation property to the Order entity
        public Product? Product { get; set; }  // Navigation property to the Product entity
  

        public OrderItem() { }
    }
}
