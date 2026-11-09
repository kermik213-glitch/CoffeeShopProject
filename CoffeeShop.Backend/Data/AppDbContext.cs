using Microsoft.EntityFrameworkCore;
using CoffeeShop.Backend.Models;

namespace CoffeeShop.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<User> Users => Set<User>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
            mb.Entity<Order>().HasOne(o => o.User)
                .WithMany().HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);
mb.Entity<Product>().HasData(
    new Product { Id = 1, Title = "Капучино", Description = "Классический кофейный напиток", Price = 190, ImageUrl = "images/capuchino.jpg", Category = "Кофе" },
    new Product { Id = 2, Title = "Латте Макиато", Description = "Нежная молочная пена с эспрессо", Price = 210, ImageUrl = "images/latte.jpg", Category = "Кофе" },
    new Product { Id = 3, Title = "Круассан", Description = "Свежая французская выпечка", Price = 150, ImageUrl = "images/cruassan.jpg", Category = "Десерты" },
    new Product { Id = 4, Title = "Эспрессо", Description = "Крепкий и бодрящий", Price = 120, ImageUrl = "images/espresso.jpg", Category = "Кофе" },
    new Product { Id = 5, Title = "Чизкейк", Description = "Нежный десерт Нью-Йорк", Price = 250, ImageUrl = "images/chisk.jpg", Category = "Десерты" }
);
            

            mb.Entity<BlogPost>().HasData(
                new BlogPost { Id = 1, Title = "Как мы выбираем зерно", Content = "Мы работаем только с проверенными фермерами и обжариваем зерно небольшими партиями.", Author = "Кофейня", CreatedAt = new DateTime(2026, 1, 1) },
                new BlogPost { Id = 2, Title = "Секреты идеального капучино", Content = "Идеальная пенка — это искусство. Мы рассказываем, как бариста добиваются её каждый день.", Author = "Бариста", CreatedAt = new DateTime(2026, 2, 1) }
            );
        }
    }
}