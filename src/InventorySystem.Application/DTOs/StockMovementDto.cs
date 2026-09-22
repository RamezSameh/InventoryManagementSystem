using InventorySystem.Domain.Entities;
namespace InventorySystem.Application.DTOs;
public record StockMovementDto(int Id,int ProductId,string ProductName,int WarehouseId,string WarehouseName,int Quantity,MovementType Type,string? Reference,string? Notes,DateTime Date);
