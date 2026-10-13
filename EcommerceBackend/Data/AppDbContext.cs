using EcommerceBackend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;  

namespace EcommerceBackend.Data
{
    public partial class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        protected override void OnModelCreating(ModelBuilder Builder)
        {
            base.OnModelCreating(Builder);
            // Configure the relationship between Cart and CartItem
            Builder.Entity<Product>()
                .Property(p => p.ProductPrice)
                .HasPrecision(18, 2); // Set precision and scale for decimal type

            Builder.Entity<Product>()
                .Property(p => p.ProductQuantity)
                .HasPrecision(18, 2); // Set precision and scale for decimal type

            Builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2); // Set precision and scale for decimal type

            Builder.Entity<Product>()
                .HasOne(p => p.ProductCategory)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.ProductCategoryId);

            Builder.Entity<OrderItem>()
                .HasOne(b=>b.Order)
                .WithMany(c => c.Items)
                .HasForeignKey(b => b.OrderId);


            Builder.Entity<Product>()
                .HasMany(p => p.ProductImages)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId);

            Builder.Entity<AppUser>()
                 .HasMany(u => u.Orders)
                 .WithOne(o => o.AppUser)
                 .HasForeignKey(o => o.UserId);

            Builder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId);

            Builder.Entity<AppUser>()
                .HasMany(u => u.Carts)
                .WithOne(c => c.AppUser)
                .HasForeignKey(c => c.UserId);

            Builder.Entity<Product>()
                .HasMany(p=> p.CartItems)
                .WithOne(ci => ci.Product)
                .HasForeignKey(ci => ci.ProductId);


            Builder.Entity<Product>()
                .HasMany(p => p.OrderItems)
                .WithOne(oi => oi.Product)
                .HasForeignKey(oi => oi.ProductId);
        }
    }
}
