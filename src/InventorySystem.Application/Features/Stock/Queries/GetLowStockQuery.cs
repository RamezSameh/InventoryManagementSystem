using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Stock.Queries;
public record LowStockDto(int ProductId,string ProductName,string SKU,int WarehouseId,string WarehouseName,int Quantity,int MinimumStock);
public record GetLowStockQuery : IRequest<List<LowStockDto>>;
public class GetLowStockQueryHandler(IAppDbContext db) : IRequestHandler<GetLowStockQuery,List<LowStockDto>> { public Task<List<LowStockDto>> Handle(GetLowStockQuery r,CancellationToken ct)=>db.StockItems.AsNoTracking().Where(x=>!x.IsDeleted&&x.Quantity<=x.Product.MinimumStock).Select(x=>new LowStockDto(x.ProductId,x.Product.Name,x.Product.SKU,x.WarehouseId,x.Warehouse.Name,x.Quantity,x.Product.MinimumStock)).ToListAsync(ct); }
