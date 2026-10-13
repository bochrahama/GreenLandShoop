using System;
using System.Collections.Generic;

namespace EcommerceBackend.Entities
{
    public class Cart
    {

        public int CartId { get; private set; }
        // Identifier for the user who owns the cart
        public string UserId { get; set; } = String.Empty;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public AppUser AppUser { get; set; } = null!; // Navigation property to the AppUser entity

        public Cart(){}
    }
    
}