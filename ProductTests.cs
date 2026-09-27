using Microsoft.EntityFrameworkCore;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.Models;
using Xunit;

namespace SmartInventoryAPI.Tests
{
    public class ProductTests
    {
        [Fact]
        public void LowStockProducts_ShouldReturnProductsBelowThreshold()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("LowStockTest")
                .Options;

            using var context = new AppDbContext(options);

            context.Products.AddRange(
                new Product
                {
                    Name = "Laptop",
                    Description = "Dell Laptop",
                    Price = 25000,
                    StockQuantity = 10
                },
                new Product
                {
                    Name = "Mouse",
                    Description = "Wireless Mouse",
                    Price = 1000,
                    StockQuantity = 3
                }
            );

            context.SaveChanges();

            // Act
            var lowStockProducts = context.Products
                .Where(p => p.StockQuantity <= 5)
                .ToList();

            // Assert
            Assert.Single(lowStockProducts);
            Assert.Equal("Mouse", lowStockProducts[0].Name);
        }
    }
}