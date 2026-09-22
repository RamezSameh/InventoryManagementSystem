using MediatR; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Products.Commands;
public record DeleteProductCommand(int Id) : IRequest;
public class DeleteProductCommandHandler(IAppDbContext db) : IRequestHandler<DeleteProductCommand> { public async Task Handle(DeleteProductCommand r,CancellationToken ct){ var p=await db.Products.FindAsync([r.Id],ct)??throw new KeyNotFoundException("Product not found."); if(p.IsDeleted) throw new KeyNotFoundException("Product not found."); p.IsDeleted=true; p.UpdatedAt=DateTime.UtcNow; await db.SaveChangesAsync(ct); } }
