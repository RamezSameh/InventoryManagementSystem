using InventorySystem.Application.Features.Stock.Commands;
using InventorySystem.Domain.Entities;
using InventorySystem.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.UnitTests.Handlers;

public class StockCommandHandlerTests
{
    [Fact]
    public async Task AddStock_PurchaseIn_UpdatesQuantityAndCreatesMovement()
    {
        await using var db = TestDb.Create();
        var (_, wh1, _, productId, _) = await TestDb.SeedCatalogAsync(db);
        var handler = new AddStockCommandHandler(db);

        var movementId = await handler.Handle(
            new AddStockCommand(productId, wh1, 10, MovementType.PurchaseIn, null, null, DestinationWarehouseId: null),
            CancellationToken.None);

        Assert.True(movementId > 0);
        var item = await db.StockItems.SingleAsync(x => x.ProductId == productId && x.WarehouseId == wh1);
        Assert.Equal(10, item.Quantity);
        Assert.Equal(1, await db.StockMovements.CountAsync());
    }

    [Fact]
    public async Task AddStock_SaleOut_MoreThanAvailable_Throws()
    {
        await using var db = TestDb.Create();
        var (_, wh1, _, productId, _) = await TestDb.SeedCatalogAsync(db);
        var handler = new AddStockCommandHandler(db);
        await handler.Handle(
            new AddStockCommand(productId, wh1, 5, MovementType.PurchaseIn, null, null, DestinationWarehouseId: null),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new AddStockCommand(productId, wh1, 20, MovementType.SaleOut, null, null, DestinationWarehouseId: null),
                CancellationToken.None));

        // Failed sale must not change the balance.
        var item = await db.StockItems.SingleAsync(x => x.ProductId == productId && x.WarehouseId == wh1);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public async Task AddStock_Transfer_CreatesTwoMovementsAndMovesQuantity()
    {
        await using var db = TestDb.Create();
        var (_, wh1, wh2, productId, _) = await TestDb.SeedCatalogAsync(db);
        var handler = new AddStockCommandHandler(db);
        await handler.Handle(
            new AddStockCommand(productId, wh1, 10, MovementType.PurchaseIn, null, null, DestinationWarehouseId: null),
            CancellationToken.None);

        var id = await handler.Handle(
            new AddStockCommand(productId, wh1, 4, MovementType.TransferOut, null, null, DestinationWarehouseId: wh2),
            CancellationToken.None);

        Assert.True(id > 0);
        Assert.Equal(6, (await db.StockItems.SingleAsync(x => x.ProductId == productId && x.WarehouseId == wh1)).Quantity);
        Assert.Equal(4, (await db.StockItems.SingleAsync(x => x.ProductId == productId && x.WarehouseId == wh2)).Quantity);

        var movements = await db.StockMovements.Where(m => m.ProductId == productId).ToListAsync();
        Assert.Equal(3, movements.Count); // 1 purchase + transfer-out + transfer-in
        Assert.Contains(movements, m => m.Type == MovementType.TransferOut && m.WarehouseId == wh1 && m.Quantity == 4);
        Assert.Contains(movements, m => m.Type == MovementType.TransferIn && m.WarehouseId == wh2 && m.Quantity == 4);
    }

    [Fact]
    public async Task AddStock_Transfer_MoreThanAvailable_ThrowsAndRollsBack()
    {
        await using var db = TestDb.Create();
        var (_, wh1, wh2, productId, _) = await TestDb.SeedCatalogAsync(db);
        var handler = new AddStockCommandHandler(db);
        await handler.Handle(
            new AddStockCommand(productId, wh1, 3, MovementType.PurchaseIn, null, null, DestinationWarehouseId: null),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new AddStockCommand(productId, wh1, 9, MovementType.TransferOut, null, null, DestinationWarehouseId: wh2),
                CancellationToken.None));

        Assert.Equal(3, (await db.StockItems.SingleAsync(x => x.ProductId == productId && x.WarehouseId == wh1)).Quantity);
        Assert.False(await db.StockItems.AnyAsync(x => x.ProductId == productId && x.WarehouseId == wh2));
        Assert.Equal(1, await db.StockMovements.CountAsync()); // only the purchase
    }
}
