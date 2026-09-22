using System.Text.Json.Serialization;
namespace InventorySystem.Application.DTOs;
public record DashboardSummaryDto([property: JsonPropertyName("totalProducts")] int TotalProducts,[property: JsonPropertyName("totalStockValue")] decimal TotalStockValue,[property: JsonPropertyName("lowStockCount")] int LowStockCount,[property: JsonPropertyName("todayMovements")] int TodayMovements);
