using Ardalis.Result;
using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using NSubstitute;
using Xunit;

namespace Lab05.Tests.Application;

public class BasketServiceTests
{
    private readonly IRepository<Basket> _repository = Substitute.For<IRepository<Basket>>();
    private readonly IAppLogger<BasketService> _logger = Substitute.For<IAppLogger<BasketService>>();
    private readonly BasketService _service;

    // xUnit constructs a fresh fixture for every test (SetUp equivalent).
    public BasketServiceTests() => _service = new BasketService(_repository, _logger);

    private void ReturnsBasket(Basket? basket) => _repository
        .FirstOrDefaultAsync(Arg.Any<ISpecification<Basket>>(), Arg.Any<CancellationToken>())
        .Returns(basket);

    [Fact]
    public async Task AddCreatesBasketWhenBuyerHasNone()
    {
        ReturnsBasket(null);
        var result = await _service.AddItemToBasket("buyer-1", 10, 20m, 2);
        Assert.Equal("buyer-1", result.BuyerId);
        Assert.Equal(2, Assert.Single(result.Items).Quantity);
        await _repository.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(result, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddUpdatesExistingBasketWithoutCreatingAnother()
    {
        var basket = new Basket("buyer-1");
        basket.AddItem(10, 20m, 2);
        ReturnsBasket(basket);
        var result = await _service.AddItemToBasket("buyer-1", 10, 20m, 3);
        Assert.Same(basket, result);
        Assert.Equal(5, Assert.Single(result.Items).Quantity);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(basket, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddPropagatesRepositoryFailure()
    {
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Basket>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Basket?>(new InvalidOperationException("storage unavailable")));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddItemToBasket("buyer-1", 10, 20m));
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRemovesExistingBasket()
    {
        var basket = new Basket("buyer-1").WithId(7);
        _repository.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(basket);
        await _service.DeleteBasketAsync(7);
        await _repository.Received(1).DeleteAsync(basket, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRejectsMissingBasket()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.DeleteBasketAsync(999));
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetQuantitiesReturnsNotFoundWithoutWriting()
    {
        ReturnsBasket(null);
        var result = await _service.SetQuantities(999, new());
        Assert.Equal(ResultStatus.NotFound, result.Status);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetQuantitiesUpdatesRequestedItemsAndPreservesOthers()
    {
        var basket = new Basket("buyer-1").WithId(7);
        basket.AddItem(10, 20m, 2);
        basket.AddItem(20, 30m, 3);
        basket.Items.First().WithId(101);
        basket.Items.Last().WithId(102);
        ReturnsBasket(basket);
        var result = await _service.SetQuantities(7, new() { ["101"] = 5 });
        Assert.True(result.IsSuccess);
        Assert.Equal(5, basket.Items.First().Quantity);
        Assert.Equal(3, basket.Items.Last().Quantity);
        await _repository.Received(1).UpdateAsync(basket, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetQuantitiesRemovesZeroQuantityEvenWithoutLogger()
    {
        var basket = new Basket("buyer-1").WithId(7);
        basket.AddItem(10, 20m, 2);
        ReturnsBasket(basket);
        var service = new BasketService(_repository, null!);
        await service.SetQuantities(7, new() { ["0"] = 0 });
        Assert.Empty(basket.Items);
        await _repository.Received(1).UpdateAsync(basket, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetQuantitiesRejectsNegativeQuantityWithoutPersisting()
    {
        var basket = new Basket("buyer-1").WithId(7);
        basket.AddItem(10, 20m, 2);
        ReturnsBasket(basket);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.SetQuantities(7, new() { ["0"] = -1 }));
        Assert.Equal(2, Assert.Single(basket.Items).Quantity);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransferDoesNothingWhenAnonymousBasketIsMissing()
    {
        ReturnsBasket(null);
        await _service.TransferBasketAsync("anonymous", "buyer-1");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransferCreatesDestinationAndDeletesAnonymousBasket()
    {
        var anonymous = new Basket("anonymous");
        anonymous.AddItem(10, 20m, 2);
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Basket>>(), Arg.Any<CancellationToken>())
            .Returns(anonymous, (Basket?)null);
        await _service.TransferBasketAsync("anonymous", "buyer-1");
        await _repository.Received(1).AddAsync(Arg.Is<Basket>(b => b != null && b.BuyerId == "buyer-1"), Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(Arg.Is<Basket>(b => b != null && b.BuyerId == "buyer-1" && b.TotalItems == 2), Arg.Any<CancellationToken>());
        await _repository.Received(1).DeleteAsync(anonymous, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransferMergesIntoExistingDestination()
    {
        var anonymous = new Basket("anonymous");
        anonymous.AddItem(10, 20m, 2);
        var buyer = new Basket("buyer-1");
        buyer.AddItem(10, 20m, 3);
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Basket>>(), Arg.Any<CancellationToken>())
            .Returns(anonymous, buyer);
        await _service.TransferBasketAsync("anonymous", "buyer-1");
        Assert.Equal(5, Assert.Single(buyer.Items).Quantity);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(buyer, Arg.Any<CancellationToken>());
        await _repository.Received(1).DeleteAsync(anonymous, Arg.Any<CancellationToken>());
    }
}
