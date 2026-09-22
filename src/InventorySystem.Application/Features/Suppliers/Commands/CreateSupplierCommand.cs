using FluentValidation; using MediatR; using InventorySystem.Application.Interfaces; using InventorySystem.Domain.Entities;
namespace InventorySystem.Application.Features.Suppliers.Commands;
public record CreateSupplierCommand(string Name,string? Phone,string? Email,string? Address) : IRequest<int>;
public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand> { public CreateSupplierCommandValidator(){ RuleFor(x=>x.Name).NotEmpty().MaximumLength(200); RuleFor(x=>x.Phone).MaximumLength(50); RuleFor(x=>x.Email).MaximumLength(200).EmailAddress().When(x=>!string.IsNullOrWhiteSpace(x.Email)); RuleFor(x=>x.Address).MaximumLength(500); } }
public class CreateSupplierCommandHandler(IAppDbContext db) : IRequestHandler<CreateSupplierCommand,int> { public async Task<int> Handle(CreateSupplierCommand r,CancellationToken ct){ var s=new Supplier{Name=r.Name,Phone=r.Phone,Email=r.Email,Address=r.Address}; db.Suppliers.Add(s); await db.SaveChangesAsync(ct); return s.Id; } }
