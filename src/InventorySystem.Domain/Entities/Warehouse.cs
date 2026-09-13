using InventorySystem.Domain.Common;
namespace InventorySystem.Domain.Entities;
public class Warehouse : BaseEntity { public string Name { get; set; } = string.Empty; public string? Location { get; set; } public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>(); }
