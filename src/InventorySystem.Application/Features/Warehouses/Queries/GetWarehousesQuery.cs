using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.DTOs; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Warehouses.Queries;
public record GetWarehousesQuery(string? Search=null) : IRequest<List<WarehouseDto>>;
public class GetWarehousesQueryHandler(IAppDbContext db) : IRequestHandler<GetWarehousesQuery,List<WarehouseDto>> { public async Task<List<WarehouseDto>> Handle(GetWarehousesQuery r,CancellationToken ct){ var q=db.Warehouses.AsNoTracking().Where(x=>!x.IsDeleted); if(!string.IsNullOrWhiteSpace(r.Search)) q=q.Where(x=>x.Name.Contains(r.Search)); return await q.OrderBy(x=>x.Name).Select(x=>new WarehouseDto(x.Id,x.Name,x.Location,x.StockItems.Count(s=>!s.IsDeleted))).ToListAsync(ct); } }
