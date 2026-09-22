using MediatR; using Microsoft.EntityFrameworkCore; using InventorySystem.Application.DTOs; using InventorySystem.Application.Interfaces;
namespace InventorySystem.Application.Features.Products.Queries;
public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;
public class GetProductByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetProductByIdQuery,ProductDto> { public async Task<ProductDto> Handle(GetProductByIdQuery r,CancellationToken ct){ var p=await db.Products.AsNoTracking().Where(x=>x.Id==r.Id&&!x.IsDeleted).Select(x=>new ProductDto(x.Id,x.Name,x.SKU,x.Barcode,x.PurchasePrice,x.SellingPrice,x.MinimumStock,x.Category.Name,x.StockItems.Where(s=>!s.IsDeleted).Sum(s=>s.Quantity))).FirstOrDefaultAsync(ct)??throw new KeyNotFoundException("Product not found."); return p; } }
