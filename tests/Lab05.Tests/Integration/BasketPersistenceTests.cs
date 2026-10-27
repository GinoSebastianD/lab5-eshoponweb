using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using NSubstitute;
using Xunit;

namespace Lab05.Tests.Integration;

public class BasketPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CatalogContext> _options;
    private readonly CatalogContext _context;
    private readonly EfRepository<Basket> _repository;

    // SetUp: each test gets an independent relational database.
    public BasketPersistenceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<CatalogContext>().UseSqlite(_connection).Options;
        _context = new SqliteCatalogContext(_options);
        _context.Database.EnsureCreated();
        _repository = new EfRepository<Basket>(_context);
    }

    [Fact]
    public async Task AddPersistsBasketAndItemsAcrossContexts()
    {
        var basket = new Basket("buyer-1");
        basket.AddItem(10, 20m, 2);
        await _repository.AddAsync(basket);
        using var readContext = new SqliteCatalogContext(_options);
        var stored = await new EfRepository<Basket>(readContext)
            .FirstOrDefaultAsync(new BasketWithItemsSpecification(basket.Id));
        Assert.NotNull(stored);
        Assert.True(stored.Id > 0);
        Assert.Equal("buyer-1", stored.BuyerId);
        var item = Assert.Single(stored.Items);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(stored.Id, item.BasketId);
    }

    [Fact]
    public async Task QueryByBuyerDoesNotReturnAnotherBuyersBasket()
    {
        await _repository.AddAsync(new Basket("buyer-1"));
        await _repository.AddAsync(new Basket("buyer-2"));
        _context.ChangeTracker.Clear();
        var result = await _repository.FirstOrDefaultAsync(new BasketWithItemsSpecification("buyer-2"));
        Assert.NotNull(result);
        Assert.Equal("buyer-2", result.BuyerId);
    }

    [Fact]
    public async Task GetByIdReturnsNullWhenBasketDoesNotExist()
    {
        Assert.Null(await _repository.GetByIdAsync(999));
    }

    [Fact]
    public async Task UpdatePersistsQuantityChange()
    {
        var basket = new Basket("buyer-1");
        basket.AddItem(10, 20m, 2);
        await _repository.AddAsync(basket);
        basket.Items.Single().SetQuantity(5);
        await _repository.UpdateAsync(basket);
        _context.ChangeTracker.Clear();
        var stored = await _repository.FirstOrDefaultAsync(new BasketWithItemsSpecification(basket.Id));
        Assert.Equal(5, Assert.Single(stored!.Items).Quantity);
    }

    [Fact]
    public async Task DeleteRemovesBasketAndDependentItems()
    {
        var basket = new Basket("buyer-1");
        basket.AddItem(10, 20m, 2);
        await _repository.AddAsync(basket);
        await _repository.DeleteAsync(basket);
        _context.ChangeTracker.Clear();
        Assert.Null(await _repository.GetByIdAsync(basket.Id));
        Assert.Equal(0, await _context.BasketItems.CountAsync());
    }

    [Fact]
    public async Task ServiceAndRepositoryPersistUpdatedQuantityTogether()
    {
        var service = new BasketService(_repository, Substitute.For<IAppLogger<BasketService>>());
        var basket = await service.AddItemToBasket("buyer-1", 10, 20m, 2);
        var itemId = basket.Items.Single().Id;
        await service.SetQuantities(basket.Id, new() { [itemId.ToString()] = 4 });
        using var readContext = new SqliteCatalogContext(_options);
        var stored = await new EfRepository<Basket>(readContext)
            .FirstOrDefaultAsync(new BasketWithItemsSpecification(basket.Id));
        Assert.Equal(4, Assert.Single(stored!.Items).Quantity);
    }

    [Fact]
    public async Task ServiceTransferPersistsMergeAndRemovesAnonymousBasket()
    {
        var service = new BasketService(_repository, Substitute.For<IAppLogger<BasketService>>());
        var anonymous = await service.AddItemToBasket("anonymous", 10, 20m, 2);
        await service.AddItemToBasket("buyer-1", 10, 20m, 3);
        await service.TransferBasketAsync("anonymous", "buyer-1");
        _context.ChangeTracker.Clear();
        var stored = await _repository.FirstOrDefaultAsync(new BasketWithItemsSpecification("buyer-1"));
        Assert.Equal(5, Assert.Single(stored!.Items).Quantity);
        Assert.Null(await _repository.GetByIdAsync(anonymous.Id));
    }

    // TearDown: close and destroy the temporary database after each test.
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
