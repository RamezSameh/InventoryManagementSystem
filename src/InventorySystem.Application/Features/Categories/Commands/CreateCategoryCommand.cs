using FluentValidation; using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.Interfaces; using InventorySystem.Domain.Entities;
namespace InventorySystem.Application.Features.Categories.Commands;
public record CreateCategoryCommand(string Name,string? Description) : IRequest<int>;
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand> { public CreateCategoryCommandValidator(){ RuleFor(x=>x.Name).NotEmpty().MaximumLength(200); RuleFor(x=>x.Description).MaximumLength(500); } }
public class CreateCategoryCommandHandler(IAppDbContext db) : IRequestHandler<CreateCategoryCommand,int> { public async Task<int> Handle(CreateCategoryCommand r,CancellationToken ct){ if(await db.Categories.AnyAsync(x=>x.Name==r.Name&&!x.IsDeleted,ct)) throw new InvalidOperationException("Category name already exists."); var c=new Category{Name=r.Name,Description=r.Description}; db.Categories.Add(c); await db.SaveChangesAsync(ct); return c.Id; } }
