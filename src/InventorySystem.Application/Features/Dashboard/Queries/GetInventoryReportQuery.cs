using MediatR;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Application.Interfaces;

namespace InventorySystem.Application.Features.Dashboard.Queries;

/// <summary>
/// Inventory value report. Values are at purchase price (same basis as the
/// dashboard TotalStockValue): value = sum(quantity * product purchase price).
/// </summary>
public record CategoryStockValueDto(string CategoryName, int ProductCount, int TotalQuantity, decimal TotalValue);
public record WarehouseStockValueDto(string WarehouseName, int ProductCount, int TotalQuantity, decimal TotalValue);
public record InventoryReportDto(List<CategoryStockValueDto> ByCategory, List<WarehouseStockValueDto> ByWarehouse, decimal TotalValue);

public record GetInventoryReportQuery : IRequest<InventoryReportDto>;

public class GetInventoryReportQueryHandler(IAppDbContext db) : IRequestHandler<GetInventoryReportQuery, InventoryReportDto>
{
    public async Task<InventoryReportDto> Handle(GetInventoryReportQuery r, CancellationToken ct)
    {
        var byCategory = await db.StockItems.AsNoTracking().Where(x => !x.IsDeleted)
            .GroupBy(x => x.Product.Category.Name)
            .Select(g => new CategoryStockValueDto(
                g.Key,
                g.Select(x => x.ProductId).Distinct().Count(),
                g.Sum(x => x.Quantity),
                g.Sum(x => (decimal)x.Quantity * x.Product.PurchasePrice)))
            .OrderByDescending(x => x.TotalValue)
            .ToListAsync(ct);
        var byWarehouse = await db.StockItems.AsNoTracking().Where(x => !x.IsDeleted)
            .GroupBy(x => x.Warehouse.Name)
            .Select(g => new WarehouseStockValueDto(
                g.Key,
                g.Select(x => x.ProductId).Distinct().Count(),
                g.Sum(x => x.Quantity),
                g.Sum(x => (decimal)x.Quantity * x.Product.PurchasePrice)))
            .OrderByDescending(x => x.TotalValue)
            .ToListAsync(ct);
        return new InventoryReportDto(byCategory, byWarehouse, byCategory.Sum(x => x.TotalValue));
    }
}
