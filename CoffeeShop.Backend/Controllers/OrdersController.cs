using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Backend.Data;
using CoffeeShop.Backend.DTOs;
using CoffeeShop.Backend.Models;
using CoffeeShop.Backend.Services;

namespace CoffeeShop.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public OrdersController(AppDbContext db) => _db = db;

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create(CreateOrderDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest("Корзина пуста");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = new Order
            {
                UserId = userId,
                AppliedStickers = dto.AppliedStickers ?? string.Empty,
                ReceiptNumber = $"R-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}"
            };

            decimal total = 0;
            foreach (var item in dto.Items)
            {
                var product = await _db.Products.FindAsync(item.ProductId);
                if (product == null || !product.IsActive || item.Quantity <= 0) continue;
                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
                total += product.Price * item.Quantity;
            }
            if (order.Items.Count == 0) return BadRequest("Нет валидных товаров");
            order.TotalPrice = total;
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return Ok(new { order.Id, order.TotalPrice, order.ReceiptNumber });
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult> GetMy()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return Ok(orders);
        }

        [HttpGet("{id}/receipt")]
        [Authorize]
        public async Task<ActionResult> GetReceipt(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = User.IsInRole("Admin");
            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            if (!isAdmin && order.UserId != userId) return Forbid();
            return Content(ReceiptService.GenerateReceipt(order), "text/plain");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetAll()
        {
            return Ok(await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync());
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.Status = status;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}