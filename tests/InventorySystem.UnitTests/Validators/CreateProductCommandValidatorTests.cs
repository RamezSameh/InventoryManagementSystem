using InventorySystem.Application.Features.Products.Commands;

namespace InventorySystem.UnitTests.Validators;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    private static CreateProductCommand Valid() =>
        new("Laptop", "SKU-001", null, null, 1000m, 1500m, 5, 1);

    [Fact]
    public void EmptyName_IsInvalid()
    {
        Assert.False(_validator.Validate(Valid() with { Name = "" }).IsValid);
    }

    [Fact]
    public void EmptySku_IsInvalid()
    {
        Assert.False(_validator.Validate(Valid() with { SKU = "  " }).IsValid);
    }

    [Fact]
    public void NegativePurchasePrice_IsInvalid()
    {
        Assert.False(_validator.Validate(Valid() with { PurchasePrice = -1m }).IsValid);
    }

    [Fact]
    public void NegativeSellingPrice_IsInvalid()
    {
        Assert.False(_validator.Validate(Valid() with { SellingPrice = -0.01m }).IsValid);
    }

    [Fact]
    public void NegativeMinimumStock_IsInvalid()
    {
        Assert.False(_validator.Validate(Valid() with { MinimumStock = -1 }).IsValid);
    }

    [Fact]
    public void ZeroCategoryId_IsInvalid()
    {
        Assert.False(_validator.Validate(Valid() with { CategoryId = 0 }).IsValid);
    }

    [Fact]
    public void FullyValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());
        Assert.True(result.IsValid);
    }
}
