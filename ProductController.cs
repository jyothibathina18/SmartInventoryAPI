using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.Models;

namespace SmartInventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _ctx;

        public ProductController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProducts(
            string? search,
            int page = 1,
            int pageSize = 2)
        {
            if (page < 1)
            {
                return BadRequest("Page must be greater than 0");
            }

            if (pageSize < 1)
            {
                return BadRequest("PageSize must be greater than 0");
            }

            var query = _ctx.Products.AsQueryable();

     
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search));
            }

        
            var totalRecords = query.Count();

        
            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                totalRecords,
                page,
                pageSize,
                products
            });
        }


        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateProduct(Product product)
        {
            _ctx.Products.Add(product);
            _ctx.SaveChanges();

            return Ok(product);
        }


        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateProduct(int id, Product product)
        {
            var existingProduct = _ctx.Products
                .FirstOrDefault(p => p.Id == id);

            if (existingProduct == null)
            {
                return NotFound("Product not found");
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;

            _ctx.SaveChanges();

            return Ok(existingProduct);
        }


       
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _ctx.Products
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            _ctx.Products.Remove(product);
            _ctx.SaveChanges();

            return Ok("Product deleted successfully");
        }


     
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult SomeUpdate(int id, Product product)
        {
            var existingProduct = _ctx.Products
                .FirstOrDefault(p => p.Id == id);

            if (existingProduct == null)
            {
                return NotFound("Product not found");
            }

            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;

            _ctx.SaveChanges();

            return Ok(existingProduct);
        }

     
        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetLowStockProducts(int threshold = 5)
        {
            var products = _ctx.Products
                .Where(p => p.StockQuantity <= threshold)
                .ToList();

            return Ok(products);
        }
    }
}