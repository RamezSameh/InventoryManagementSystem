using FluentValidation; using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.Interfaces; using InventorySystem.Domain.Entities;
namespace InventorySystem.Application.Features.Warehouses.Commands;
public record CreateWarehouseCommand(string Name,string? Location) : IRequest<int>;
public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand> { public CreateWarehouseCommandValidator(){ RuleFor(x=>x.Name).NotEmpty().MaximumLength(200); RuleFor(x=>x.Location).MaximumLength(300); } }
public class CreateWarehouseCommandHandler(IAppDbContext db) : IRequestHandler<CreateWarehouseCommand,int> { public async Task<int> Handle(CreateWarehouseCommand r,CancellationToken ct){ if(await db.Warehouses.AnyAsync(x=>x.Name==r.Name&&!x.IsDeleted,ct)) throw new InvalidOperationException("Warehouse name already exists."); var w=new Warehouse{Name=r.Name,Location=r.Location}; db.Warehouses.Add(w); await db.SaveChangesAsync(ct); return w.Id; } }
