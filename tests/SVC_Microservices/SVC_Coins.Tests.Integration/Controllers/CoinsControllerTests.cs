using System.Net;
using System.Net.Http.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SVC_Coins.Models.Entities;
using SVC_Coins.Models.Input;
using SVC_Coins.Models.Output;
using SVC_Coins.Tests.Integration.Factories;

namespace SVC_Coins.Tests.Integration.Controllers;

public class CoinsControllerTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory),
        IClassFixture<CustomWebApplicationFactory>
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task InsertCoin_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var coinNew = new CoinNew { Name = "Bitcoin", Symbol = "BTC" };

        // Act
        var response = await Client.PostAsJsonAsync("/coins/insert", coinNew);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var dbContext = GetDbContext();
        var coinsList = await dbContext.Coins.ToListAsync();
        coinsList.Should().ContainSingle(c => c.Name == coinNew.Name && c.Symbol == coinNew.Symbol);
    }

    [Fact]
    public async Task InsertCoin_ReturnsConflict_WhenCoinAlreadyExists()
    {
        // Arrange
        var coinNew = new CoinNew { Name = "Bitcoin", Symbol = "BTC" };
        await InsertCoinsAsync([new CoinsEntity { Name = coinNew.Name, Symbol = coinNew.Symbol }]);

        // Act
        var response = await Client.PostAsJsonAsync("/coins/insert", coinNew);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var message = await response.Content.ReadAsStringAsync();
        message.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetAllCoins_ShouldReturnOkWithList()
    {
        // Arrange
        await InsertCoinsAsync([new CoinsEntity { Name = "Bitcoin", Symbol = "BTC" }]);

        // Act
        var response = await Client.GetAsync("/coins/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var coinsList = await response.Content.ReadFromJsonAsync<IEnumerable<Coin>>();
        coinsList.Should().NotBeNull();
        coinsList.Should().ContainSingle(c => c.Name == "Bitcoin" && c.Symbol == "BTC");
    }

    [Fact]
    public async Task DeleteCoin_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var insertedCoins = await InsertCoinsAsync(
            [new CoinsEntity { Name = "Bitcoin", Symbol = "BTC" }]
        );
        var insertedCoinId = insertedCoins.First().Id;

        // Act
        var response = await Client.DeleteAsync($"/coins/{insertedCoinId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var dbContext2 = GetDbContext();
        var coinsList = await dbContext2.Coins.ToListAsync();
        coinsList.Should().NotContain(c => c.Id == insertedCoinId);
    }

    [Fact]
    public async Task DeleteCoin_ReturnsNotFound_WhenCoinDoesNotExist()
    {
        // Arrange
        var nonExistentId = int.MaxValue;

        // Act
        var response = await Client.DeleteAsync($"/coins/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var message = await response.Content.ReadAsStringAsync();
        message.Should().Contain("not found");
    }

    [Fact]
    public async Task InsertTradingPair_ReturnsOkWithId_WhenSuccessful()
    {
        // Arrange
        var insertedCoins = await InsertCoinsAsync(
            [
                new CoinsEntity { Name = "Bitcoin", Symbol = "BTC" },
                new CoinsEntity { Name = "Ethereum", Symbol = "ETH" },
            ]
        );
        var mainCoinId = insertedCoins.First().Id;
        var quoteCoinId = insertedCoins.Last().Id;

        var tradingPairNew = new TradingPairNew
        {
            IdCoinMain = mainCoinId,
            IdCoinQuote = quoteCoinId,
        };

        // Act
        var response = await Client.PostAsJsonAsync("/coins/tradingPairs/insert", tradingPairNew);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var insertedId = await response.Content.ReadFromJsonAsync<int>();
        insertedId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task InsertTradingPair_ReturnsBadRequest_WhenInsertionFails()
    {
        // Attempt to insert a trading pair for non-existent coins
        var tradingPairNew = _fixture.Create<TradingPairNew>();

        // Act
        var response = await Client.PostAsJsonAsync("/coins/tradingPairs/insert", tradingPairNew);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var message = await response.Content.ReadAsStringAsync();
        message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetQuoteCoinsPrioritized_ShouldReturnOkWithPrioritizedList()
    {
        // Arrange
        await InsertCoinsAsync(
            [
                new CoinsEntity
                {
                    Name = "Bitcoin",
                    Symbol = "BTC",
                    QuoteCoinPriority = 2,
                },
                new CoinsEntity
                {
                    Name = "Ethereum",
                    Symbol = "ETH",
                    QuoteCoinPriority = 1,
                },
                new CoinsEntity
                {
                    Name = "Tether",
                    Symbol = "USDT",
                    QuoteCoinPriority = 3,
                },
            ]
        );

        // Act
        var response = await Client.GetAsync("/coins/quoteCoinsPrioritized");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var prioritizedCoins = await response.Content.ReadFromJsonAsync<IEnumerable<Coin>>();
        prioritizedCoins.Should().NotBeNull();
        prioritizedCoins.Should().HaveCount(3);

        var orderedCoins = prioritizedCoins!.ToList();
        orderedCoins[0].Name.Should().Be("Ethereum");
        orderedCoins[1].Name.Should().Be("Bitcoin");
        orderedCoins[2].Name.Should().Be("Tether");
    }
}
