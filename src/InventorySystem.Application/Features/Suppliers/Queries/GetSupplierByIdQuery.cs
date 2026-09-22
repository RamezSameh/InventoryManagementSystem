using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.DTOs; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Suppliers.Queries;
public record GetSupplierByIdQuery(int Id) : IRequest<SupplierDto>;
public class GetSupplierByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetSupplierByIdQuery,SupplierDto> { public async Task<SupplierDto> Handle(GetSupplierByIdQuery r,CancellationToken ct){ var s=await db.Suppliers.AsNoTracking().Where(x=>x.Id==r.Id&&!x.IsDeleted).Select(x=>new SupplierDto(x.Id,x.Name,x.Phone,x.Email,x.Address)).FirstOrDefaultAsync(ct)??throw new KeyNotFoundException("Supplier not found."); return s; } }
