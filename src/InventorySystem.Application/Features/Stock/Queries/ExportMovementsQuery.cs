using MediatR;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Application.Common.Csv;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Features.Stock.Queries;

/// <summary>Returns the stock-movement log as a CSV document, with the same filters as the list.</summary>
public record ExportMovementsQuery(int? ProductId = null, int? WarehouseId = null, MovementType? Type = null, DateTime? From = null, DateTime? To = null) : IRequest<string>;

public class ExportMovementsQueryHandler(IAppDbContext db) : IRequestHandler<ExportMovementsQuery, string>
{
    public async Task<string> Handle(ExportMovementsQuery r, CancellationToken ct)
    {
        var q = db.StockMovements.AsNoTracking().Where(x => !x.IsDeleted);
        if (r.ProductId.HasValue) q = q.Where(x => x.ProductId == r.ProductId.Value);
        if (r.WarehouseId.HasValue) q = q.Where(x => x.WarehouseId == r.WarehouseId.Value);
        if (r.Type.HasValue) q = q.Where(x => x.Type == r.Type.Value);
        if (r.From.HasValue) q = q.Where(x => x.Date >= r.From.Value);
        if (r.To.HasValue) q = q.Where(x => x.Date <= r.To.Value);
        var items = await q.OrderByDescending(x => x.Date)
            .Select(x => new
            {
                x.Id,
                x.Date,
                ProductName = x.Product.Name,
                WarehouseName = x.Warehouse.Name,
                x.Type,
                x.Quantity,
                x.Reference,
                x.CreatedBy,
                x.Notes
            })
            .ToListAsync(ct);
        return CsvHelper.Build(
            new[] { "Id", "Date", "Product", "Warehouse", "Type", "Quantity", "Reference", "CreatedBy", "Notes" },
            items.Select(x => new string?[]
            {
                x.Id.ToString(), CsvHelper.FormatDate(x.Date), x.ProductName, x.WarehouseName,
                x.Type.ToString(), x.Quantity.ToString(), x.Reference, x.CreatedBy, x.Notes
            }));
    }
}
