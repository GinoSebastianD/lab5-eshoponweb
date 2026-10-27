using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Xunit;

namespace Lab05.Tests.Domain;

public class BasketTests
{
    [Fact]
    public void NewBasketIsEmptyAndKeepsBuyer()
    {
        var basket = new Basket("buyer-1");
        Assert.Equal("buyer-1", basket.BuyerId);
        Assert.Empty(basket.Items);
        Assert.Equal(0, basket.TotalItems);
    }

    [Fact]
    public void AddItemUsesDefaultQuantity()
    {
        var basket = new Basket("buyer-1");
        basket.AddItem(10, 25m);
        var item = Assert.Single(basket.Items);
        Assert.Equal(10, item.CatalogItemId);
        Assert.Equal(25m, item.UnitPrice);
        Assert.Equal(1, item.Quantity);
    }

    [Fact]
    public void RepeatedProductAccumulatesQuantityWithoutDuplicatingItem()
    {
        var basket = new Basket("buyer-1");
        basket.AddItem(10, 25m, 2);
        basket.AddItem(10, 25m, 3);
        Assert.Equal(5, Assert.Single(basket.Items).Quantity);
    }

    [Fact]
    public void TotalItemsSumsQuantitiesOfDifferentProducts()
    {
        var basket = new Basket("buyer-1");
        basket.AddItem(10, 25m, 2);
        basket.AddItem(20, 30m, 3);
        Assert.Equal(2, basket.Items.Count);
        Assert.Equal(5, basket.TotalItems);
    }

    [Fact]
    public void RemoveEmptyItemsPreservesPositiveQuantities()
    {
        var basket = new Basket("buyer-1");
        basket.AddItem(10, 25m, 0);
        basket.AddItem(20, 30m, 2);
        basket.RemoveEmptyItems();
        Assert.Equal(20, Assert.Single(basket.Items).CatalogItemId);
        Assert.Equal(2, basket.TotalItems);
    }

    [Fact]
    public void SetNewBuyerIdTransfersOwnership()
    {
        var basket = new Basket("anonymous");
        basket.SetNewBuyerId("buyer-1");
        Assert.Equal("buyer-1", basket.BuyerId);
    }
}
