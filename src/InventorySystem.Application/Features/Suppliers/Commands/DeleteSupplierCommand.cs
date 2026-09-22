using MediatR; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Suppliers.Commands;
public record DeleteSupplierCommand(int Id) : IRequest;
public class DeleteSupplierCommandHandler(IAppDbContext db) : IRequestHandler<DeleteSupplierCommand> { public async Task Handle(DeleteSupplierCommand r,CancellationToken ct){ var s=await db.Suppliers.FindAsync([r.Id],ct)??throw new KeyNotFoundException("Supplier not found."); if(s.IsDeleted) throw new KeyNotFoundException("Supplier not found."); s.IsDeleted=true; s.UpdatedAt=DateTime.UtcNow; await db.SaveChangesAsync(ct); } }
