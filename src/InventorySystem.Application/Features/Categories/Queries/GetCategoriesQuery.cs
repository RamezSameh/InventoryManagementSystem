using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.DTOs; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Categories.Queries;
public record GetCategoriesQuery(string? Search=null) : IRequest<List<CategoryDto>>;
public class GetCategoriesQueryHandler(IAppDbContext db) : IRequestHandler<GetCategoriesQuery,List<CategoryDto>> { public async Task<List<CategoryDto>> Handle(GetCategoriesQuery r,CancellationToken ct){ var q=db.Categories.AsNoTracking().Where(x=>!x.IsDeleted); if(!string.IsNullOrWhiteSpace(r.Search)) q=q.Where(x=>x.Name.Contains(r.Search)); return await q.OrderBy(x=>x.Name).Select(x=>new CategoryDto(x.Id,x.Name,x.Description,x.Products.Count(p=>!p.IsDeleted))).ToListAsync(ct); } }
