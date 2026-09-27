using Microsoft.EntityFrameworkCore;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.Models;
using Xunit;

namespace SmartInventoryAPI.Tests
{
    public class OrderTests
    {
        [Fact]
        public void CancelOrder_ShouldRestoreProductStock()
        {
           
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("OrderCancelTest")
                .Options;

            using var context = new AppDbContext(options);

            var product = new Product
            {
                Name = "Laptop",
                Description = "Dell Laptop",
                Price = 25000,
                StockQuantity = 5
            };

            context.Products.Add(product);
            context.SaveChanges();

            var order = new Order
            {
                UserId = 3,
                OrderDate = DateTime.Now,
                TotalAmount = 50000,
                Status = "Pending"
            };

            context.Orders.Add(order);
            context.SaveChanges();

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = 2,
                Price = product.Price
            };

            context.OrderItems.Add(orderItem);

            product.StockQuantity -= orderItem.Quantity;

            context.SaveChanges();

            product.StockQuantity += orderItem.Quantity;
            order.Status = "Cancelled";

            context.SaveChanges();

            Assert.Equal(5, product.StockQuantity);
            Assert.Equal("Cancelled", order.Status);
        }
    }
}