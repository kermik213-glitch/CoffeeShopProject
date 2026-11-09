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
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly TokenService _tokens;
        public AuthController(AppDbContext db, TokenService tokens) { _db = db; _tokens = tokens; }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email и пароль обязательны");
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Пользователь с таким email уже существует");

            var user = new User
            {
                Email = dto.Email,
                Name = string.IsNullOrWhiteSpace(dto.Name) ? dto.Email : dto.Name,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                Role = "User"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return new AuthResponseDto(_tokens.GenerateToken(user), user.Email, user.Name, user.Role);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Неверный email или пароль");
            return new AuthResponseDto(_tokens.GenerateToken(user), user.Email, user.Name, user.Role);
        }
    }
}