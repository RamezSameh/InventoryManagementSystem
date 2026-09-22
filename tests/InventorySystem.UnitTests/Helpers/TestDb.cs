using InventorySystem.Domain.Entities;
using InventorySystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.UnitTests.Helpers;

public static class TestDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    public static async Task<(int CategoryId, int WarehouseId, int Warehouse2Id, int ProductId, string Sku)> SeedCatalogAsync(
        AppDbContext db, int minimumStock = 5)
    {
        var category = new Category { Name = "TestCat-" + Guid.NewGuid().ToString("N") };
        var wh1 = new Warehouse { Name = "WH1-" + Guid.NewGuid().ToString("N") };
        var wh2 = new Warehouse { Name = "WH2-" + Guid.NewGuid().ToString("N") };
        db.Categories.Add(category);
        db.Warehouses.AddRange(wh1, wh2);
        await db.SaveChangesAsync();

        var product = new Product
        {
            Name = "Widget",
            SKU = "SKU-" + Guid.NewGuid().ToString("N"),
            PurchasePrice = 10m,
            SellingPrice = 15m,
            MinimumStock = minimumStock,
            CategoryId = category.Id
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        return (category.Id, wh1.Id, wh2.Id, product.Id, product.SKU);
    }
}
