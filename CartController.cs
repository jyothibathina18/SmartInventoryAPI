using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.Models;
using System.Security.Claims;

namespace SmartInventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _ctx;

        public CartController(AppDbContext ctx)
        {
            _ctx = ctx;
        }


        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = _ctx.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var product = _ctx.Products.FirstOrDefault(
                p => p.Id == productId);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            if (quantity <= 0)
            {
                return BadRequest("Quantity must be greater than 0");
            }

            if (product.StockQuantity < quantity)
            {
                return BadRequest("Not enough stock");
            }

            var cart = _ctx.Carts.FirstOrDefault(
                c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _ctx.Carts.Add(cart);
                _ctx.SaveChanges();
            }

            var cartItem = _ctx.CartItems.FirstOrDefault(
                c => c.CartId == cart.Id &&
                     c.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                };

                _ctx.CartItems.Add(cartItem);
            }

            _ctx.SaveChanges();

            return Ok(new
            {
                message = "Product added to cart",
                cartId = cart.Id,
                productId = productId,
                quantity = cartItem.Quantity
            });
        }


        [HttpGet]
        public IActionResult GetMyCart()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var cart = _ctx.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            var result = new
            {
                cartId = cart.Id,
                userId = cart.UserId,

                items = cart.CartItems.Select(ci => new
                {
                    cartItemId = ci.Id,
                    productId = ci.ProductId,
                    productName = ci.Product.Name,
                    quantity = ci.Quantity,
                    price = ci.Product.Price,
                    total = ci.Product.Price * ci.Quantity
                })
            };

            return Ok(result);
        }


        [HttpDelete("{productId}")]
        public IActionResult RemoveFromCart(int productId)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var cart = _ctx.Carts.FirstOrDefault(
                c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            var item = _ctx.CartItems.FirstOrDefault(
                c => c.CartId == cart.Id &&
                     c.ProductId == productId);

            if (item == null)
            {
                return NotFound("Product not found in cart");
            }

            _ctx.CartItems.Remove(item);
            _ctx.SaveChanges();

            return Ok("Product removed from cart");
        }
    }
}