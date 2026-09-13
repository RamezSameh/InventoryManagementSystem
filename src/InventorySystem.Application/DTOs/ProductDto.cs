namespace InventorySystem.Application.DTOs;
public record ProductDto(int Id,string Name,string SKU,string? Barcode,decimal PurchasePrice,decimal SellingPrice,int MinimumStock,string CategoryName,int TotalStock);
