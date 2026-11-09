using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Backend.Data;
using CoffeeShop.Backend.DTOs;

namespace CoffeeShop.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public StatsController(AppDbContext db) => _db = db;

               [HttpGet("summary")]
        public async Task<ActionResult<StatsSummaryDto>> Summary()
        {
            var totalOrders = await _db.Orders.CountAsync();
            
       
            var prices = await _db.Orders.Select(o => o.TotalPrice).ToListAsync();
    
            var totalRevenue = prices.Sum(); 
            
            var totalUsers = await _db.Users.CountAsync();
            var totalProducts = await _db.Products.CountAsync(p => p.IsActive);
            var avg = totalOrders > 0 ? totalRevenue / totalOrders : 0m;
            
            return new StatsSummaryDto(totalOrders, totalRevenue, totalUsers, totalProducts, avg);
        }

        [HttpGet("sales-by-day")]
        public async Task<ActionResult<List<SalesByDayDto>>> SalesByDay([FromQuery] int days = 14)
        {
            var from = DateTime.UtcNow.Date.AddDays(-days + 1);
            

            var orders = await _db.Orders
                .Where(o => o.CreatedAt >= from)
                .ToListAsync(); 

       
            var data = orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { 
                    Date = g.Key, 
                    Orders = g.Count(), 
                    Revenue = g.Sum(o => o.TotalPrice) 
                })
                .ToList();

            var result = new List<SalesByDayDto>();
            for (int i = 0; i < days; i++)
            {
                var d = from.AddDays(i);
                var f = data.FirstOrDefault(x => x.Date == d);
                result.Add(new SalesByDayDto(d.ToString("dd.MM"), f?.Orders ?? 0, f?.Revenue ?? 0));
            }
            return result;
        }

        [HttpGet("top-products")]
        public async Task<ActionResult<List<TopProductDto>>> TopProducts([FromQuery] int top = 5)
        {
   
            var items = await _db.OrderItems
                .Include(i => i.Product)
                .ToListAsync();

          
            return items
                .GroupBy(i => new { i.ProductId, Title = i.Product?.Title ?? "Неизвестный" })
                .Select(g => new TopProductDto(
                    g.Key.ProductId,
                    g.Key.Title,
                    g.Sum(i => i.Quantity),
                    g.Sum(i => i.Price * i.Quantity)))
                .OrderByDescending(x => x.Quantity)
                .Take(top)
                .ToList();
        }
    }
}