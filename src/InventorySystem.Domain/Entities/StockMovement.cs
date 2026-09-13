using InventorySystem.Domain.Common;
namespace InventorySystem.Domain.Entities;
public enum MovementType { PurchaseIn=1, SaleOut, TransferIn, TransferOut, Adjustment, Return }
public class StockMovement : BaseEntity { public int ProductId { get; set; } public int WarehouseId { get; set; } public int Quantity { get; set; } public MovementType Type { get; set; } public string? Reference { get; set; } public DateTime Date { get; set; } = DateTime.UtcNow; public string? Notes { get; set; } public Product Product { get; set; } = null!; public Warehouse Warehouse { get; set; } = null!; }
