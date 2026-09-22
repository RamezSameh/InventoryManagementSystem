using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.DTOs; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Categories.Queries;
public record GetCategoryByIdQuery(int Id) : IRequest<CategoryDto>;
public class GetCategoryByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetCategoryByIdQuery,CategoryDto> { public async Task<CategoryDto> Handle(GetCategoryByIdQuery r,CancellationToken ct){ var c=await db.Categories.AsNoTracking().Where(x=>x.Id==r.Id&&!x.IsDeleted).Select(x=>new CategoryDto(x.Id,x.Name,x.Description,x.Products.Count(p=>!p.IsDeleted))).FirstOrDefaultAsync(ct)??throw new KeyNotFoundException("Category not found."); return c; } }
