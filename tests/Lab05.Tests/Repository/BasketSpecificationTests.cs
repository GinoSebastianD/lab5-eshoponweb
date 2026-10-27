using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Xunit;

namespace Lab05.Tests.Repository;

public class BasketSpecificationTests
{
    [Theory]
    [InlineData(7, true)]
    [InlineData(8, false)]
    public void FilterByIdMatchesOnlyTheRequestedBasket(int id, bool expected)
    {
        var basket = new Basket("buyer-1").WithId(id);
        var specification = new BasketWithItemsSpecification(7);
        Assert.Equal(expected, specification.IsSatisfiedBy(basket));
        Assert.Single(specification.IncludeExpressions);
    }

    [Theory]
    [InlineData("buyer-1", true)]
    [InlineData("buyer-2", false)]
    public void FilterByBuyerMatchesOnlyTheRequestedOwner(string buyer, bool expected)
    {
        var basket = new Basket(buyer);
        var specification = new BasketWithItemsSpecification("buyer-1");
        Assert.Equal(expected, specification.IsSatisfiedBy(basket));
        Assert.Single(specification.IncludeExpressions);
    }
}
