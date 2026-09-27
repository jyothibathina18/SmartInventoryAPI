using Microsoft.EntityFrameworkCore;
using SmartInventoryAPI.Models;

namespace SmartInventoryAPI.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>options)
            :base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 4,
                    Name = "Seed Customer",
                    Email = "seedcustomer@gmail.com",
                    Password = "123456",
                    Role = "Customer"
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 8,
                    Name = "Seed Laptop",
                    Description = "Sample Laptop",
                    Price = 30000,
                    StockQuantity = 10
                },
                new Product
                {
                    Id = 9,
                    Name = "Seed Mobile",
                    Description = "Sample Mobile",
                    Price = 15000,
                    StockQuantity = 20
                }
            );
        }



    }
}
