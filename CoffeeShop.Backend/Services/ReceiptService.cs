using System.Text;
using CoffeeShop.Backend.Models;

namespace CoffeeShop.Backend.Services
{
    public static class ReceiptService
    {
        public static string GenerateReceipt(Order order)
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================");
            sb.AppendLine("       КОФЕЙНЯ «AROMA»          ");
            sb.AppendLine("================================");
            sb.AppendLine($"Чек №: {order.ReceiptNumber}");
            sb.AppendLine($"Дата: {order.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}");
            sb.AppendLine("--------------------------------");
            foreach (var item in order.Items)
            {
                var title = item.Product?.Title ?? $"Товар #{item.ProductId}";
                sb.AppendLine($"{title}");
                sb.AppendLine($"  {item.Quantity} x {item.Price:F2} ₽ = {item.Quantity * item.Price:F2} ₽");
            }
            sb.AppendLine("--------------------------------");
            if (!string.IsNullOrEmpty(order.AppliedStickers))
                sb.AppendLine($"Пожелания: {order.AppliedStickers}");
            sb.AppendLine($"ИТОГО: {order.TotalPrice:F2} ₽");
            sb.AppendLine("================================");
            sb.AppendLine("     Спасибо за покупку!        ");
            sb.AppendLine("================================");
            return sb.ToString();
        }
    }
}