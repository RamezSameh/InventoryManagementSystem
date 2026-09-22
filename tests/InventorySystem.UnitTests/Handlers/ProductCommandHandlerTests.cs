using InventorySystem.Application.Features.Products.Commands;
using InventorySystem.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.UnitTests.Handlers;

public class ProductCommandHandlerTests
{
    [Fact]
    public async Task CreateProduct_PersistsProduct()
    {
        await using var db = TestDb.Create();
        var (categoryId, _, _, _, _) = await TestDb.SeedCatalogAsync(db);
        var handler = new CreateProductCommandHandler(db);

        var id = await handler.Handle(
            new CreateProductCommand("Keyboard", "SKU-KB-1", null, null, 20m, 35m, 3, categoryId),
            CancellationToken.None);

        Assert.True(id > 0);
        var saved = await db.Products.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal("Keyboard", saved.Name);
        Assert.Equal("SKU-KB-1", saved.SKU);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_Throws()
    {
        await using var db = TestDb.Create();
        var (categoryId, _, _, _, sku) = await TestDb.SeedCatalogAsync(db);
        var handler = new CreateProductCommandHandler(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new CreateProductCommand("Copy", sku, null, null, 1m, 2m, 0, categoryId),
                CancellationToken.None));
    }
}
