using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Xunit;

namespace Lab05.Tests.Domain;

public class BasketItemTests
{
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
