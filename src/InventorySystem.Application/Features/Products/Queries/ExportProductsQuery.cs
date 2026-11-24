using MediatR;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Application.Common.Csv;
using InventorySystem.Application.Interfaces;

namespace InventorySystem.Application.Features.Products.Queries;

/// <summary>Returns all (non-deleted) products as a CSV document, same search as the list.</summary>
public record ExportProductsQuery(string? Search = null) : IRequest<string>;

public class ExportProductsQueryHandler(IAppDbContext db) : IRequestHandler<ExportProductsQuery, string>
{
    public async Task<string> Handle(ExportProductsQuery r, CancellationToken ct)
    {
        var q = db.Products.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(r.Search))
            q = q.Where(x => x.Name.Contains(r.Search) || x.SKU.Contains(r.Search));
        var items = await q.OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.SKU,
                x.Barcode,
                CategoryName = x.Category.Name,
                SupplierName = x.Supplier != null ? x.Supplier.Name : null,
                x.PurchasePrice,
                x.SellingPrice,
                x.MinimumStock,
                TotalStock = x.StockItems.Where(s => !s.IsDeleted).Sum(s => s.Quantity),
                x.Description
            })
            .ToListAsync(ct);
        return CsvHelper.Build(
            new[] { "Id", "Name", "SKU", "Barcode", "Category", "Supplier", "PurchasePrice", "SellingPrice", "MinimumStock", "TotalStock", "Description" },
            items.Select(x => new string?[]
            {
                x.Id.ToString(), x.Name, x.SKU, x.Barcode, x.CategoryName, x.SupplierName,
                CsvHelper.FormatDecimal(x.PurchasePrice), CsvHelper.FormatDecimal(x.SellingPrice),
                x.MinimumStock.ToString(), x.TotalStock.ToString(), x.Description
            }));
    }
}
