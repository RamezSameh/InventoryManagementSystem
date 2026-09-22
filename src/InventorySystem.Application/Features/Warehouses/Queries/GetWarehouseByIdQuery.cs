using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.DTOs; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Warehouses.Queries;
public record GetWarehouseByIdQuery(int Id) : IRequest<WarehouseDto>;
public class GetWarehouseByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetWarehouseByIdQuery,WarehouseDto> { public async Task<WarehouseDto> Handle(GetWarehouseByIdQuery r,CancellationToken ct){ var w=await db.Warehouses.AsNoTracking().Where(x=>x.Id==r.Id&&!x.IsDeleted).Select(x=>new WarehouseDto(x.Id,x.Name,x.Location,x.StockItems.Count(s=>!s.IsDeleted))).FirstOrDefaultAsync(ct)??throw new KeyNotFoundException("Warehouse not found."); return w; } }
