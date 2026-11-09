using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Backend.Data;

namespace CoffeeShop.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult> GetAll() => Ok(await _db.Users
            .Select(u => new { u.Id, u.Email, u.Name, u.Role, u.CreatedAt })
            .ToListAsync());

        [HttpPut("{id}/role")]
        public async Task<IActionResult> SetRole(int id, [FromBody] string role)
        {
            if (role != "User" && role != "Admin") return BadRequest("Неверная роль");
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();
            u.Role = role;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}