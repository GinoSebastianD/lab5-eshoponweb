using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Xunit;

namespace Lab05.Tests.Domain;

public class BasketItemTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void ConstructorAcceptsNonNegativeQuantity(int quantity)
    {
        var item = new BasketItem(7, quantity, 15m);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(7, item.CatalogItemId);
        Assert.Equal(15m, item.UnitPrice);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ConstructorRejectsNegativeQuantity(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BasketItem(7, quantity, 15m));
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(3, 5)]
    [InlineData(int.MaxValue - 2, int.MaxValue)]
    public void AddQuantityAcceptsValuesWithinRemainingCapacity(int increment, int expected)
    {
        var item = new BasketItem(1, 2, 10m);
        item.AddQuantity(increment);
        Assert.Equal(expected, item.Quantity);
    }

    [Fact]
    public void AddQuantityRejectsNegativeIncrementWithoutChangingState()
    {
        var item = new BasketItem(1, 2, 10m);
        Assert.Throws<ArgumentOutOfRangeException>(() => item.AddQuantity(-1));
        Assert.Equal(2, item.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void SetQuantityReplacesPreviousQuantity(int quantity)
    {
        var item = new BasketItem(1, 2, 10m);
        item.SetQuantity(quantity);
        Assert.Equal(quantity, item.Quantity);
    }

    [Fact]
    public void SetQuantityRejectsNegativeValueWithoutChangingState()
    {
        var item = new BasketItem(1, 2, 10m);
        Assert.Throws<ArgumentOutOfRangeException>(() => item.SetQuantity(-1));
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void AddQuantityRejectsOverflowAndPreservesQuantity()
    {
        // Arrange
        var item = new BasketItem(1, int.MaxValue, 10m);

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => item.AddQuantity(1));
        Assert.Equal(int.MaxValue, item.Quantity);
    }
}
