using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.Models;
using System.Data;
using System.Security.Claims;

namespace SmartInventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _ctx;

        public OrderController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpPost]
        public IActionResult PlaceOrder()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            using var transaction = _ctx.Database.BeginTransaction(
                IsolationLevel.Serializable);

            try
            {
                var cart = _ctx.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                {
                    return BadRequest("Cart is empty");
                }

                foreach (var item in cart.CartItems)
                {
                    if (item.Quantity > item.Product.StockQuantity)
                    {
                        return BadRequest(
                            $"Insufficient stock for {item.Product.Name}");
                    }
                }

                decimal totalAmount = 0;

                foreach (var item in cart.CartItems)
                {
                    totalAmount += item.Product.Price * item.Quantity;
                }

                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    OrderDate = DateTime.Now,
                    Status = "Pending"
                };

                _ctx.Orders.Add(order);
                _ctx.SaveChanges();

                foreach (var item in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    };

                    _ctx.OrderItems.Add(orderItem);

                    item.Product.StockQuantity -= item.Quantity;
                }

                _ctx.CartItems.RemoveRange(cart.CartItems);

                _ctx.SaveChanges();

                transaction.Commit();

                return Ok(new
                {
                    message = "Order placed successfully",
                    orderId = order.Id,
                    totalAmount = order.TotalAmount
                });
            }
            catch
            {
                transaction.Rollback();

                return StatusCode(
                    500,
                    "Something went wrong while placing the order");
            }
        }


        [HttpGet("myorders")]
        public IActionResult GetMyOrders()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var orders = _ctx.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .Select(o => new
                {
                    orderId = o.Id,
                    userId = o.UserId,
                    orderDate = o.OrderDate,
                    status = o.Status,
                    totalAmount = o.TotalAmount,

                    items = o.OrderItems.Select(oi => new
                    {
                        productId = oi.ProductId,
                        productName = oi.Product.Name,
                        quantity = oi.Quantity,
                        price = oi.Price,
                        total = oi.Price * oi.Quantity
                    })
                })
                .ToList();

            if (!orders.Any())
            {
                return NotFound("No orders found");
            }

            return Ok(orders);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetOrders(string? search)
        {
            var orders = _ctx.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o =>
                    o.OrderItems.Any(oi =>
                        oi.Product.Name.Contains(search)));
            }

            var result = orders
                .Select(o => new
                {
                    orderId = o.Id,
                    userId = o.UserId,
                    orderDate = o.OrderDate,
                    totalAmount = o.TotalAmount,
                    status = o.Status,

                    items = o.OrderItems.Select(oi => new
                    {
                        productId = oi.ProductId,
                        productName = oi.Product.Name,
                        quantity = oi.Quantity,
                        price = oi.Price
                    })
                })
                .ToList();

            return Ok(result);
        }


        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateOrderStatus(int id, string status)
        {
            var order = _ctx.Orders
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            order.Status = status;

            _ctx.SaveChanges();

            return Ok(new
            {
                message = "Order status updated successfully",
                orderId = order.Id,
                status = order.Status
            });
        }
        [HttpPut("{id}/cancel")]
        [Authorize]
        public IActionResult CancelOrder(int id)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var order = _ctx.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            if (User.IsInRole("Customer") && order.UserId != userId)
            {
                return Forbid();
            }

            if (order.Status == "Cancelled")
            {
                return BadRequest("Order is already cancelled");
            }

            foreach (var item in order.OrderItems)
            {
                item.Product.StockQuantity += item.Quantity;
            }

            order.Status = "Cancelled";

            _ctx.SaveChanges();

            return Ok(new
            {
                message = "Order cancelled successfully",
                orderId = order.Id,
                status = order.Status
            });
        }



        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetOrderById(int id)
        {
            var order = _ctx.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == id)
                .Select(o => new
                {
                    orderId = o.Id,
                    userId = o.UserId,
                    orderDate = o.OrderDate,
                    status = o.Status,
                    totalAmount = o.TotalAmount,

                    items = o.OrderItems.Select(oi => new
                    {
                        productId = oi.ProductId,
                        productName = oi.Product.Name,
                        quantity = oi.Quantity,
                        price = oi.Price,
                        total = oi.Price * oi.Quantity
                    })
                })
                .FirstOrDefault();

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order);
        }
    }
}