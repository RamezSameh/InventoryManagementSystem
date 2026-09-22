using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.DTOs; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Suppliers.Queries;
public record GetSuppliersQuery(string? Search=null) : IRequest<List<SupplierDto>>;
public class GetSuppliersQueryHandler(IAppDbContext db) : IRequestHandler<GetSuppliersQuery,List<SupplierDto>> { public async Task<List<SupplierDto>> Handle(GetSuppliersQuery r,CancellationToken ct){ var q=db.Suppliers.AsNoTracking().Where(x=>!x.IsDeleted); if(!string.IsNullOrWhiteSpace(r.Search)) q=q.Where(x=>x.Name.Contains(r.Search)); return await q.OrderBy(x=>x.Name).Select(x=>new SupplierDto(x.Id,x.Name,x.Phone,x.Email,x.Address)).ToListAsync(ct); } }
