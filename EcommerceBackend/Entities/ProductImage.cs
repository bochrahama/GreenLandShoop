using System;

namespace EcommerceBackend.Entities
{
    public class ProductImage
    {
        //guid type is used for unique identifier for each image like a primary key in the database ex : 3fa85f64-5717-4562-b3fc-2c963f66afa6
        public Guid Id { get; set; } = Guid.NewGuid();
        // we use Guid for ProductId to ensure that each product can have a unique identifier, which is especially useful in distributed systems or when integrating with other services. It helps avoid collisions and ensures that each product can be uniquely identified across different systems.
        public int ProductId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation property (optional)
        
        public Product? Product { get; set; }
        // Parameterless constructor for EF Core
        public ProductImage() { }
    }
}