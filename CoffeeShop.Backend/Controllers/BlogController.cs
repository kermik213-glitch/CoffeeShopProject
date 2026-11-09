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
    public class BlogController : ControllerBase
    {
        private readonly AppDbContext _db;
        public BlogController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<BlogPostDto>>> GetAll()
        {
            return await _db.BlogPosts
                .Where(p => p.Published)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new BlogPostDto(p.Id, p.Title, p.Content, p.ImageUrl, p.Author, p.CreatedAt))
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPostDto>> Get(int id)
        {
            var p = await _db.BlogPosts.FindAsync(id);
            if (p == null || !p.Published) return NotFound();
            return new BlogPostDto(p.Id, p.Title, p.Content, p.ImageUrl, p.Author, p.CreatedAt);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogPostDto>> Create(CreateBlogPostDto dto)
        {
            var p = new BlogPost { Title = dto.Title, Content = dto.Content, ImageUrl = dto.ImageUrl };
            _db.BlogPosts.Add(p);
            await _db.SaveChangesAsync();
            return new BlogPostDto(p.Id, p.Title, p.Content, p.ImageUrl, p.Author, p.CreatedAt);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, CreateBlogPostDto dto)
        {
            var p = await _db.BlogPosts.FindAsync(id);
            if (p == null) return NotFound();
            p.Title = dto.Title; p.Content = dto.Content; p.ImageUrl = dto.ImageUrl;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.BlogPosts.FindAsync(id);
            if (p == null) return NotFound();
            _db.BlogPosts.Remove(p);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}