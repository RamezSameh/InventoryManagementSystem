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
        // NOTE: GroupBy مع Distinct().Count() جوّه الـ Select مبيتترجمش لـ SQL في
        // EF Core، فبيضرب InvalidOperationException (والـ middleware بتحوّله 400).
        // فبنجيب الصفوف الأول باستعلام بسيط، والتجميع بيتم في الذاكرة.
        var rows = await db.StockItems.AsNoTracking().Where(x => !x.IsDeleted)
            .Select(x => new
            {
                CategoryName = x.Product.Category.Name,
                WarehouseName = x.Warehouse.Name,
                x.ProductId,
                x.Quantity,
                x.Product.PurchasePrice
            })
            .ToListAsync(ct);
        var byCategory = rows.GroupBy(x => x.CategoryName)
            .Select(g => new CategoryStockValueDto(
                g.Key,
                g.Select(x => x.ProductId).Distinct().Count(),
                g.Sum(x => x.Quantity),
                g.Sum(x => (decimal)x.Quantity * x.PurchasePrice)))
            .OrderByDescending(x => x.TotalValue)
            .ToList();
        var byWarehouse = rows.GroupBy(x => x.WarehouseName)
            .Select(g => new WarehouseStockValueDto(
                g.Key,
                g.Select(x => x.ProductId).Distinct().Count(),
                g.Sum(x => x.Quantity),
                g.Sum(x => (decimal)x.Quantity * x.PurchasePrice)))
            .OrderByDescending(x => x.TotalValue)
            .ToList();
        return new InventoryReportDto(byCategory, byWarehouse, byCategory.Sum(x => x.TotalValue));
    }
}
