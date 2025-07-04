using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SVC_Coins.Domain.Entities;
using SVC_Coins.Domain.ValueObjects;
using SVC_Coins.Infrastructure;
using SVC_Coins.Repositories;

namespace SVC_Coins.Tests.Unit.Repositories;

public class CoinsRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CoinsDbContext _seedContext;
    private readonly CoinsDbContext _actContext;
    private readonly CoinsDbContext _assertContext;
    private readonly CoinsRepository _testedRepository;

    public CoinsRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CoinsDbContext>().UseSqlite(_connection).Options;

        _seedContext = new CoinsDbContext(options);
        _actContext = new CoinsDbContext(options);
        _assertContext = new CoinsDbContext(options);
        _seedContext.Database.EnsureCreated();

        _testedRepository = new CoinsRepository(_actContext);
    }

    [Fact]
    public async Task GetAllCoinsWithRelations_ReturnsAllCoinsWithRelations()
    {
        // Arrange
        await SeedDatabase(addTradingPairs: true);

        // Act
        var result = await _testedRepository.GetAllCoinsWithRelations();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        // Verify all coins exist with correct IDs
        result.Should().Contain(c => c.Id == 1);
        result.Should().Contain(c => c.Id == 2);
        result.Should().Contain(c => c.Id == 3);

        // Verify important properties for first coin
        var firstCoin = result.First(c => c.Id == 1);
        firstCoin.Symbol.Should().Be("BTC");
        firstCoin.Name.Should().Be("Bitcoin");
        firstCoin.TradingPairs.Should().HaveCount(1);

        // Verify trading pair details for first coin
        var tradingPair = firstCoin.TradingPairs.First();
        tradingPair.CoinQuote.Id.Should().Be(3);
        tradingPair.Exchanges.Should().HaveCount(1);
        tradingPair.Exchanges.First().Name.Should().Be("Binance");

        // Verify second coin's trading pairs
        var secondCoin = result.First(c => c.Id == 2);
        secondCoin.TradingPairs.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllCoinsWithRelations_WhenNoCoinsExist_ReturnsEmpty()
    {
        // Act
        var result = await _testedRepository.GetAllCoinsWithRelations();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCoinsByIds_ReturnsCorrectCoinsWithoutRelations()
    {
        // Arrange
        // Seed with relations, but GetCoinsByIds should not load them
        await SeedDatabase(addTradingPairs: true);
        var coinIds = new[] { 1, 2 };

        // Act
        var result = await _testedRepository.GetCoinsByIds(coinIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        // Verify coins exist with correct IDs
        result.Should().Contain(c => c.Id == 1);
        result.Should().Contain(c => c.Id == 2);

        // Verify important properties and ABSENCE of relations
        var firstCoin = result.First(c => c.Id == 1);
        firstCoin.Symbol.Should().Be("BTC");
        firstCoin.Name.Should().Be("Bitcoin");
        firstCoin.TradingPairs.Should().BeNullOrEmpty();

        var secondCoin = result.First(c => c.Id == 2);
        secondCoin.Symbol.Should().Be("ETH");
        secondCoin.TradingPairs.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetCoinsByIds_WhenSomeIdsDontExist_ReturnsOnlyExistingCoins()
    {
        // Arrange
        await SeedDatabase(); // Seeds 1, 2, 3
        var coinIds = new[] { 1, 999, 2 }; // 999 does not exist

        // Act
        var result = await _testedRepository.GetCoinsByIds(coinIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == 1);
        result.Should().Contain(c => c.Id == 2);
        result.Should().NotContain(c => c.Id == 3);
        result.Should().NotContain(c => c.Id == 999);
    }

    [Fact]
    public async Task GetCoinsByIds_WhenNoIdsExist_ReturnsEmpty()
    {
        // Arrange
        await SeedDatabase();
        var coinIds = new[] { 998, 999 };

        // Act
        var result = await _testedRepository.GetCoinsByIds(coinIds);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCoinsByIds_WhenInputIsEmpty_ReturnsEmpty()
    {
        // Arrange
        await SeedDatabase();
        var coinIds = Array.Empty<int>();

        // Act
        var result = await _testedRepository.GetCoinsByIds(coinIds);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCoinsByIds_WhenDatabaseIsEmpty_ReturnsEmpty()
    {
        // Arrange
        var coinIds = new[] { 1, 2 };

        // Act
        var result = await _testedRepository.GetCoinsByIds(coinIds);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCoinsByIdsWithRelations_ReturnsCorrectCoinsWithTradingPairs()
    {
        // Arrange
        await SeedDatabase(addTradingPairs: true);
        var coinIds = new[] { 1, 2, 3 };

        // Act
        var result = await _testedRepository.GetCoinsByIdsWithRelations(coinIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        // Verify all coins exist with correct IDs
        result.Should().Contain(c => c.Id == 1);
        result.Should().Contain(c => c.Id == 2);
        result.Should().Contain(c => c.Id == 3);

        // Verify important properties for first coin
        var firstCoin = result.First(c => c.Id == 1);
        firstCoin.Symbol.Should().Be("BTC");
        firstCoin.Name.Should().Be("Bitcoin");
        firstCoin.TradingPairs.Should().HaveCount(1);

        // Verify trading pair details for first coin
        var tradingPair = firstCoin.TradingPairs.First();
        tradingPair.CoinQuote.Id.Should().Be(3);
        tradingPair.Exchanges.Should().HaveCount(1);
        tradingPair.Exchanges.First().Name.Should().Be("Binance");

        // Verify second coin's trading pairs
        var secondCoin = result.First(c => c.Id == 2);
        secondCoin.TradingPairs.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCoinsByIdsWithRelations_WhenNoCoinsExist_ReturnsEmpty()
    {
        // Act
        var result = await _testedRepository.GetCoinsByIdsWithRelations([1, 2]);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCoinsBySymbolNamePairs_WhenPairsExist_ReturnsMatchingCoins()
    {
        // Arrange
        await SeedDatabase();
        var pairs = new List<CoinSymbolNamePair>
        {
            new() { Symbol = "BTC", Name = "Bitcoin" },
            new() { Symbol = "ETH", Name = "Ethereum" },
            new() { Symbol = "USDT", Name = "Tether" },
        };

        // Act
        var result = await _testedRepository.GetCoinsBySymbolNamePairs(pairs);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        // Verify all coins exist with correct Symbols
        result.Should().Contain(c => c.Symbol == "BTC");
        result.Should().Contain(c => c.Symbol == "ETH");
        result.Should().Contain(c => c.Symbol == "USDT");

        // Verify important properties for first coin
        var firstCoin = result.First(c => c.Id == 1);
        firstCoin.Symbol.Should().Be("BTC");
        firstCoin.Name.Should().Be("Bitcoin");
    }

    [Fact]
    public async Task GetCoinsBySymbolNamePairs_WhenNoPairsMatch_ReturnsEmpty()
    {
        // Arrange
        var pairs = new List<CoinSymbolNamePair>
        {
            new() { Symbol = "ETH", Name = "Ethereum" },
            new() { Symbol = "USDT", Name = "Tether" },
        };

        // Act
        var result = await _testedRepository.GetCoinsBySymbolNamePairs(pairs);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMissingCoinIds_WhenSomeIdsAreMissing_ReturnsMissingIds()
    {
        // Arrange
        await SeedDatabase();
        var coinIdsToCheck = new HashSet<int> { 1, 2, 999, 1000 }; // Existing IDs: 1, 2, 3

        // Act
        var result = await _testedRepository.GetMissingCoinIds(coinIdsToCheck);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(999);
        result.Should().Contain(1000);
    }

    [Fact]
    public async Task GetMissingCoinIds_WhenAllIdsExist_ReturnsEmpty()
    {
        // Arrange
        await SeedDatabase();
        var coinIdsToCheck = new HashSet<int> { 1, 2, 3 }; // All exist

        // Act
        var result = await _testedRepository.GetMissingCoinIds(coinIdsToCheck);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMissingCoinIds_WhenInputIsEmpty_ReturnsEmpty()
    {
        // Arrange
        await SeedDatabase();
        var coinIdsToCheck = new HashSet<int>();

        // Act
        var result = await _testedRepository.GetMissingCoinIds(coinIdsToCheck);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMissingCoinIds_WhenDatabaseIsEmpty_ReturnsAllInputIds()
    {
        // Arrange
        var coinIdsToCheck = new HashSet<int> { 1, 2, 3 };

        // Act
        var result = await _testedRepository.GetMissingCoinIds(coinIdsToCheck);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(coinIdsToCheck);
    }

    [Fact]
    public async Task InsertCoins_AddsEntitiesToDatabase()
    {
        // Arrange
        var coinsToInsert = TestData.CoinsForInsertion;

        // Act
        await _testedRepository.InsertCoins(coinsToInsert);

        // Assert
        var coinsInDb = await _assertContext.Coins.ToListAsync();
        coinsInDb.Should().HaveCount(coinsToInsert.Count());
        coinsInDb[0].Symbol.Should().Be(TestData.CoinsForInsertion.First().Symbol);
        coinsInDb[1].Symbol.Should().Be(TestData.CoinsForInsertion.Last().Symbol);
    }

    [Fact]
    public async Task InsertCoins_ReturnsInsertedCoins()
    {
        // Act
        var result = await _testedRepository.InsertCoins(TestData.CoinsForInsertion);

        // Assert
        result.Should().HaveCount(2);
        result.First().Id.Should().Be(1);
        result.Last().Id.Should().Be(2);
        result.First().Symbol.Should().Be("BTC");
        result.Last().Symbol.Should().Be("USDT");
    }

    [Fact]
    public async Task InsertCoins_WhenCollectionIsEmpty_ShouldDoNothing()
    {
        // Act
        var result = await _testedRepository.InsertCoins([]);

        // Assert
        var coinsInDb = await _assertContext.Coins.ToListAsync();
        coinsInDb.Should().BeEmpty();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateCoins_UpdatesCoinsDataInDatabase()
    {
        // Arrange
        await SeedDatabase();

        var coins = await _actContext.Coins.ToListAsync();
        foreach (var coin in coins)
        {
            if (coin.Id == 1)
            {
                coin.MarketCapUsd = int.MaxValue;
                coin.PriceUsd = "100000";
                coin.PriceChangePercentage24h = 100;
            }
            else if (coin.Id == 2)
            {
                coin.MarketCapUsd = int.MaxValue;
                coin.PriceUsd = "1";
                coin.PriceChangePercentage24h = 100;
            }
        }

        // Act
        await _testedRepository.UpdateCoins(coins);

        // Assert
        var coinsInDb = await _assertContext.Coins.ToListAsync();
        coinsInDb.First(c => c.Id == 1).MarketCapUsd.Should().Be(int.MaxValue);
        coinsInDb.First(c => c.Id == 1).PriceUsd.Should().Be("100000");
        coinsInDb.First(c => c.Id == 1).PriceChangePercentage24h.Should().Be(100);
        coinsInDb.First(c => c.Id == 2).MarketCapUsd.Should().Be(int.MaxValue);
        coinsInDb.First(c => c.Id == 2).PriceUsd.Should().Be("1");
        coinsInDb.First(c => c.Id == 2).PriceChangePercentage24h.Should().Be(100);
    }

    [Fact]
    public async Task UpdateCoins_ReturnsUpdatedCoins()
    {
        // Arrange
        await SeedDatabase();

        var coins = await _actContext.Coins.ToListAsync();
        foreach (var coin in coins)
        {
            if (coin.Id == 1)
            {
                coin.MarketCapUsd = int.MaxValue;
                coin.PriceUsd = "100000";
                coin.PriceChangePercentage24h = 100;
            }
            else if (coin.Id == 2)
            {
                coin.MarketCapUsd = int.MaxValue;
                coin.PriceUsd = "1";
                coin.PriceChangePercentage24h = 100;
            }
        }

        // Act
        var result = await _testedRepository.UpdateCoins(coins);

        // Assert
        result.Should().HaveCount(3);
        result.First(c => c.Id == 1).MarketCapUsd.Should().Be(int.MaxValue);
        result.First(c => c.Id == 1).PriceUsd.Should().Be("100000");
        result.First(c => c.Id == 1).PriceChangePercentage24h.Should().Be(100);
        result.First(c => c.Id == 2).MarketCapUsd.Should().Be(int.MaxValue);
        result.First(c => c.Id == 2).PriceUsd.Should().Be("1");
        result.First(c => c.Id == 2).PriceChangePercentage24h.Should().Be(100);
    }

    [Fact]
    public async Task UpdateCoins_WhenCollectionIsEmpty_ShouldDoNothing()
    {
        // Act
        var result = await _testedRepository.UpdateCoins([]);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckCoinExists_WhenCoinExists_ReturnsTrue()
    {
        // Arrange
        await SeedDatabase();
        const int existingCoinId = 1;

        // Act
        var result = await _testedRepository.CheckCoinExists(existingCoinId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckCoinExists_WhenCoinDoesNotExist_ReturnsFalse()
    {
        // Arrange
        await SeedDatabase();
        const int nonExistingCoinId = 999;

        // Act
        var result = await _testedRepository.CheckCoinExists(nonExistingCoinId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckCoinExists_WhenDatabaseIsEmpty_ReturnsFalse()
    {
        // Arrange
        const int anyCoinId = 1;

        // Act
        var result = await _testedRepository.CheckCoinExists(anyCoinId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCoinsNotReferencedByTradingPairs_DeletesCoinsWithoutTradingPairs()
    {
        // Arrange
        await SeedDatabase();

        // Act
        await _testedRepository.DeleteCoinsNotReferencedByTradingPairs();

        // Assert
        var coinsInDb = await _assertContext.Coins.ToListAsync();
        coinsInDb.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteCoinsNotReferencedByTradingPairs_WhenAllCoinsAreReferenced_DoesNothing()
    {
        // Arrange
        await SeedDatabase(addTradingPairs: true);

        // Act
        await _testedRepository.DeleteCoinsNotReferencedByTradingPairs();

        // Assert
        var coinsInDb = await _assertContext.Coins.ToListAsync();
        coinsInDb.Should().HaveCount(3);
    }

    [Fact]
    public async Task DeleteAllCoinsWithRelations_DeletesAllCoinsAndTradingPairs()
    {
        // Arrange
        await SeedDatabase(addTradingPairs: true);

        // Act
        await _testedRepository.DeleteAllCoinsWithRelations();

        // Assert
        var coinsInDb = await _assertContext.Coins.ToListAsync();
        coinsInDb.Should().BeEmpty();
        var tradingPairsInDb = await _assertContext.TradingPairs.ToListAsync();
        tradingPairsInDb.Should().BeEmpty();
        var exchangesInDb = await _assertContext.Exchanges.ToListAsync();
        exchangesInDb.Should().HaveCount(3); // Exchanges are automatically seeded and should remain
    }

    private async Task SeedDatabase(bool addTradingPairs = false)
    {
        var coins = TestData.GetCoins();
        await _seedContext.Coins.AddRangeAsync(coins);

        if (addTradingPairs)
        {
            // Get the automatically seeded exchanges from the database
            var exchanges = await _seedContext.Exchanges.ToListAsync();
            var tradingPairs = TestData.GetTradingPairs(exchanges);
            await _seedContext.TradingPairs.AddRangeAsync(tradingPairs);
        }

        await _seedContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _seedContext.Dispose();
            _actContext.Dispose();
            _assertContext.Dispose();
            _connection.Dispose();
        }
    }

    public static class TestData
    {
        public static IEnumerable<CoinsEntity> GetCoins() =>
            [
                new CoinsEntity
                {
                    Symbol = "BTC",
                    Name = "Bitcoin",
                    IsFiat = false,
                    IdCoinGecko = "bitcoin",
                    IsStablecoin = false,
                },
                new CoinsEntity
                {
                    Symbol = "ETH",
                    Name = "Ethereum",
                    IsFiat = false,
                    IdCoinGecko = null,
                    IsStablecoin = false,
                },
                new CoinsEntity
                {
                    Symbol = "USDT",
                    Name = "Tether",
                    IsFiat = false,
                    IdCoinGecko = "tether",
                    IsStablecoin = true,
                },
            ];

        public static IEnumerable<TradingPairsEntity> GetTradingPairs(
            List<ExchangesEntity> exchanges
        ) =>
            [
                new TradingPairsEntity
                {
                    IdCoinMain = 1,
                    IdCoinQuote = 3,
                    Exchanges = [exchanges.First(e => e.Name == "Binance")],
                },
                new TradingPairsEntity
                {
                    IdCoinMain = 2,
                    IdCoinQuote = 1,
                    Exchanges =
                    [
                        exchanges.First(e => e.Name == "Binance"),
                        exchanges.First(e => e.Name == "Bybit"),
                    ],
                },
            ];

        public static readonly IEnumerable<CoinsEntity> CoinsForInsertion =
        [
            new CoinsEntity
            {
                Symbol = "BTC",
                Name = "Bitcoin",
                IsFiat = false,
                IdCoinGecko = "bitcoin",
                IsStablecoin = false,
            },
            new CoinsEntity
            {
                Symbol = "USDT",
                Name = "Tether",
                IsFiat = false,
                IdCoinGecko = null,
                IsStablecoin = true,
            },
        ];
    }
}
