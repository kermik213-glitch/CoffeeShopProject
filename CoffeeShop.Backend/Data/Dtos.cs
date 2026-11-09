namespace CoffeeShop.Backend.DTOs
{
    public record RegisterDto(string Email, string Password, string Name);
    public record LoginDto(string Email, string Password);
    public record AuthResponseDto(string Token, string Email, string Name, string Role);
    public record CreateOrderItemDto(int ProductId, int Quantity);
    public record CreateOrderDto(List<CreateOrderItemDto> Items, string AppliedStickers);
    public record ProductDto(int Id, string Title, string Description, decimal Price, string ImageUrl, string Category, bool IsActive);
    public record BlogPostDto(int Id, string Title, string Content, string ImageUrl, string Author, DateTime CreatedAt);
    public record CreateBlogPostDto(string Title, string Content, string ImageUrl);
    public record StatsSummaryDto(int TotalOrders, decimal TotalRevenue, int TotalUsers, int TotalProducts, decimal AvgOrderValue);
    public record SalesByDayDto(string Date, int Orders, decimal Revenue);
    public record TopProductDto(int ProductId, string Title, int Quantity, decimal Revenue);
}