using System;
using System.Collections.Generic;

namespace EcommerceBackend.Entities
{

    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString();
        //UserId type is string because it is the primary key of the AspNetUsers table which is of type string
        public string UserId { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount => CalculateTotal();
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string ShippingAddress { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; } 
        public Order() { }

        private decimal CalculateTotal()
        {
            decimal total = 0m;
            foreach (var item in Items)
            {
                total += item.TotalPrice;
            }
            return total;
        }
    }
}
