using InventorySystem.Application.Features.Stock.Commands;
using InventorySystem.Domain.Entities;

namespace InventorySystem.UnitTests.Validators;

public class AddStockCommandValidatorTests
{
    private readonly AddStockCommandValidator _validator = new();

    private static AddStockCommand Valid() =>
        new(1, 1, 5, MovementType.PurchaseIn, null, null, DestinationWarehouseId: null);

    [Fact]
    public void ZeroQuantity_IsInvalid()
    {
        Assert.False(_validator.Validate(Valid() with { Quantity = 0 }).IsValid);
    }

    [Fact]
    public void NegativeQuantity_IsInvalid()
    {
        Assert.False(_validator.Validate(Valid() with { Quantity = -3 }).IsValid);
    }

    [Fact]
    public void TransferOut_WithoutDestinationWarehouseId_IsInvalid()
    {
        var cmd = new AddStockCommand(1, 1, 5, MovementType.TransferOut, null, null, DestinationWarehouseId: null);
        var result = _validator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddStockCommand.DestinationWarehouseId));
    }

    [Fact]
    public void TransferOut_WithDestinationWarehouseId_IsValid()
    {
        var cmd = new AddStockCommand(1, 1, 5, MovementType.TransferOut, null, null, DestinationWarehouseId: 2);
        Assert.True(_validator.Validate(cmd).IsValid);
    }

    [Fact]
    public void ValidPurchaseIn_IsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }
}
