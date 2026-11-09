using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Backend.Data;
using CoffeeShop.Backend.DTOs;
using CoffeeShop.Backend.Models;

namespace CoffeeShop.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductsController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] bool includeInactive = false)
        {
            var q = _db.Products.AsQueryable();
            if (!includeInactive) q = q.Where(p => p.IsActive);
            return await q.OrderBy(p => p.Id)
                .Select(p => new ProductDto(p.Id, p.Title, p.Description, p.Price, p.ImageUrl, p.Category, p.IsActive))
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            return new ProductDto(p.Id, p.Title, p.Description, p.Price, p.ImageUrl, p.Category, p.IsActive);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> Create(ProductDto dto)
        {
            var p = new Product
            {
                Title = dto.Title, Description = dto.Description, Price = dto.Price,
                ImageUrl = dto.ImageUrl, Category = dto.Category, IsActive = dto.IsActive
            };
            _db.Products.Add(p);
            await _db.SaveChangesAsync();
            return new ProductDto(p.Id, p.Title, p.Description, p.Price, p.ImageUrl, p.Category, p.IsActive);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, ProductDto dto)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            p.Title = dto.Title; p.Description = dto.Description; p.Price = dto.Price;
            p.ImageUrl = dto.ImageUrl; p.Category = dto.Category; p.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            p.IsActive = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}