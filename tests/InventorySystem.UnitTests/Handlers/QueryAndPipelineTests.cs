using FluentValidation;
using InventorySystem.Application.Common.Behaviors;
using InventorySystem.Application.Features.Products.Commands;
using InventorySystem.Application.Features.Stock.Commands;
using InventorySystem.Application.Features.Stock.Queries;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Infrastructure.Persistence;
using InventorySystem.UnitTests.Helpers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.UnitTests.Handlers;

public class LowStockQueryTests
{
    [Fact]
    public async Task GetLowStock_ReturnsItemsAtOrBelowMinimum()
    {
        await using var db = TestDb.Create();
        var (_, wh1, _, productId, sku) = await TestDb.SeedCatalogAsync(db, minimumStock: 10);
        var stock = new AddStockCommandHandler(db);
        await stock.Handle(
            new AddStockCommand(productId, wh1, 20, MovementType.PurchaseIn, null, null, DestinationWarehouseId: null),
            CancellationToken.None);
        await stock.Handle(
            new AddStockCommand(productId, wh1, 15, MovementType.SaleOut, null, null, DestinationWarehouseId: null),
            CancellationToken.None); // 5 left, minimum is 10

        var result = await new GetLowStockQueryHandler(db).Handle(new GetLowStockQuery(), CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(sku, item.SKU);
        Assert.Equal(5, item.Quantity);
        Assert.Equal(10, item.MinimumStock);
    }

    [Fact]
    public async Task GetLowStock_ExcludesWellStockedItems()
    {
        await using var db = TestDb.Create();
        var (_, wh1, _, productId, _) = await TestDb.SeedCatalogAsync(db, minimumStock: 10);
        var stock = new AddStockCommandHandler(db);
        await stock.Handle(
            new AddStockCommand(productId, wh1, 50, MovementType.PurchaseIn, null, null, DestinationWarehouseId: null),
            CancellationToken.None);

        var result = await new GetLowStockQueryHandler(db).Handle(new GetLowStockQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}

public class ValidationBehaviorTests
{
    private static IMediator BuildMediator(AppDbContext db)
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
        services.AddValidatorsFromAssembly(typeof(CreateProductCommand).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IAppDbContext>(_ => db);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task InvalidCommand_ThroughPipeline_ThrowsValidationException()
    {
        await using var db = TestDb.Create();
        var mediator = BuildMediator(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            mediator.Send(new CreateProductCommand("", "", null, null, -5m, -5m, -1, 0)));
    }

    [Fact]
    public async Task ValidCommand_ThroughPipeline_PersistsProduct()
    {
        await using var db = TestDb.Create();
        db.Categories.Add(new Category { Name = "PipeCat" });
        await db.SaveChangesAsync();
        var mediator = BuildMediator(db);

        var id = await mediator.Send(new CreateProductCommand("Mouse", "SKU-PIPE", null, null, 5m, 9m, 1, 1));

        Assert.True(id > 0);
        Assert.NotNull(await db.Products.FindAsync(id));
    }
}
