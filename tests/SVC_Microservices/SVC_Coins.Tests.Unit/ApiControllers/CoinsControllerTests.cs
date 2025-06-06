using FluentResults;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Errors;
using SVC_Coins.ApiContracts.Requests;
using SVC_Coins.ApiContracts.Requests.CoinCreation;
using SVC_Coins.ApiContracts.Responses;
using SVC_Coins.ApiControllers;
using SVC_Coins.Services;

namespace SVC_Coins.Tests.Unit.ApiControllers;

public class CoinsControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICoinsService> _mockCoinsService;
    private readonly CoinsController _testedController;

    public CoinsControllerTests()
    {
        _fixture = new Fixture();

        _mockCoinsService = new Mock<ICoinsService>();
        _testedController = new CoinsController(_mockCoinsService.Object);
    }

    [Fact]
    public async Task GetCoins_WithIds_CallsServiceAndReturnsOk()
    {
        // Arrange
        var ids = _fixture.CreateMany<int>(3).ToList();
        var expectedCoins = _fixture.CreateMany<Coin>(3).ToList();

        _mockCoinsService
            .Setup(service => service.GetCoinsByIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(expectedCoins);

        // Act
        var result = await _testedController.GetCoins(ids);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(expectedCoins);

        _mockCoinsService.Verify(service => service.GetCoinsByIds(ids), Times.Once);
        _mockCoinsService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCoins_WithoutIds_CallsServiceAndReturnsOk()
    {
        // Arrange
        var expectedCoins = _fixture.CreateMany<Coin>(5).ToList();

        _mockCoinsService.Setup(service => service.GetAllCoins()).ReturnsAsync(expectedCoins);

        // Act
        var result = await _testedController.GetCoins([]);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(expectedCoins);

        _mockCoinsService.Verify(service => service.GetAllCoins(), Times.Once);
        _mockCoinsService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCoins_WithIdsButNoMatchingCoinsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var ids = _fixture.CreateMany<int>(3).ToList();

        _mockCoinsService
            .Setup(service => service.GetCoinsByIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([]);

        // Act
        var result = await _testedController.GetCoins(ids);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(Array.Empty<Coin>());

        _mockCoinsService.Verify(service => service.GetCoinsByIds(ids), Times.Once);
    }

    [Fact]
    public async Task CreateCoins_OnSuccess_CallsServiceAndReturnsOk()
    {
        // Arrange
        var requests = _fixture.CreateMany<CoinCreationRequest>(3).ToList();
        var createdCoins = _fixture.CreateMany<Coin>(3);
        var successResult = Result.Ok(createdCoins);

        _mockCoinsService
            .Setup(service =>
                service.CreateCoinsWithTradingPairs(It.IsAny<IEnumerable<CoinCreationRequest>>())
            )
            .ReturnsAsync(successResult);

        // Act
        var result = await _testedController.CreateCoins(requests);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(createdCoins);

        _mockCoinsService.Verify(
            service => service.CreateCoinsWithTradingPairs(requests),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateCoins_OnFailure_CallsServiceAndReturnsBadRequest()
    {
        // Arrange
        var requests = _fixture.CreateMany<CoinCreationRequest>(3).ToList();
        var errorMessage = "Validation failed";
        var failureResult = Result.Fail<IEnumerable<Coin>>(
            new GenericErrors.BadRequestError(errorMessage)
        );

        _mockCoinsService
            .Setup(service =>
                service.CreateCoinsWithTradingPairs(It.IsAny<IEnumerable<CoinCreationRequest>>())
            )
            .ReturnsAsync(failureResult);

        // Act
        var result = await _testedController.CreateCoins(requests);

        // Assert
        result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Which.Value.Should()
            .BeOfType<ProblemDetails>()
            .Which.Detail.Should()
            .Contain(errorMessage);

        _mockCoinsService.Verify(
            service => service.CreateCoinsWithTradingPairs(requests),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateQuoteCoins_OnSuccess_CallsServiceAndReturnsOk()
    {
        // Arrange
        var requests = _fixture.CreateMany<QuoteCoinCreationRequest>(3).ToList();
        var createdQuoteCoins = _fixture.CreateMany<TradingPairCoinQuote>(3);
        var successResult = Result.Ok(createdQuoteCoins);

        _mockCoinsService
            .Setup(service =>
                service.CreateQuoteCoins(It.IsAny<IEnumerable<QuoteCoinCreationRequest>>())
            )
            .ReturnsAsync(successResult);

        // Act
        var result = await _testedController.CreateQuoteCoins(requests);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(createdQuoteCoins);

        _mockCoinsService.Verify(service => service.CreateQuoteCoins(requests), Times.Once);
    }

    [Fact]
    public async Task CreateQuoteCoins_OnFailure_CallsServiceAndReturnsBadRequest()
    {
        // Arrange
        var requests = _fixture.CreateMany<QuoteCoinCreationRequest>(3).ToList();
        var errorMessage = "Quote coin validation failed";
        var failureResult = Result.Fail<IEnumerable<TradingPairCoinQuote>>(
            new GenericErrors.BadRequestError(errorMessage)
        );

        _mockCoinsService
            .Setup(service =>
                service.CreateQuoteCoins(It.IsAny<IEnumerable<QuoteCoinCreationRequest>>())
            )
            .ReturnsAsync(failureResult);

        // Act
        var result = await _testedController.CreateQuoteCoins(requests);

        // Assert
        result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Which.Value.Should()
            .BeOfType<ProblemDetails>()
            .Which.Detail.Should()
            .Contain(errorMessage);

        _mockCoinsService.Verify(service => service.CreateQuoteCoins(requests), Times.Once);
    }

    [Fact]
    public async Task CreateQuoteCoins_WithEmptyRequest_CallsServiceAndReturnsOk()
    {
        // Arrange
        var emptyRequests = Enumerable.Empty<QuoteCoinCreationRequest>();
        var successResult = Result.Ok(Enumerable.Empty<TradingPairCoinQuote>());

        _mockCoinsService
            .Setup(service =>
                service.CreateQuoteCoins(It.IsAny<IEnumerable<QuoteCoinCreationRequest>>())
            )
            .ReturnsAsync(successResult);

        // Act
        var result = await _testedController.CreateQuoteCoins(emptyRequests);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(Enumerable.Empty<TradingPairCoinQuote>());

        _mockCoinsService.Verify(service => service.CreateQuoteCoins(emptyRequests), Times.Once);
    }

    [Fact]
    public async Task UpdateMarketData_OnSuccess_CallsServiceAndReturnsOk()
    {
        // Arrange
        var requests = _fixture.CreateMany<CoinMarketDataUpdateRequest>(3);
        var updatedCoins = _fixture.CreateMany<Coin>(3);
        var successResult = Result.Ok(updatedCoins);

        _mockCoinsService
            .Setup(service =>
                service.UpdateCoinsMarketData(It.IsAny<IEnumerable<CoinMarketDataUpdateRequest>>())
            )
            .ReturnsAsync(successResult);

        // Act
        var result = await _testedController.UpdateMarketData(requests);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(updatedCoins);

        _mockCoinsService.Verify(service => service.UpdateCoinsMarketData(requests), Times.Once);
    }

    [Fact]
    public async Task UpdateMarketData_OnNotFound_CallsServiceAndReturnsNotFound()
    {
        // Arrange
        var requests = _fixture.CreateMany<CoinMarketDataUpdateRequest>(3);
        var errorMessage = "Coin not found";
        var failureResult = Result.Fail<IEnumerable<Coin>>(
            new GenericErrors.NotFoundError(errorMessage)
        );

        _mockCoinsService
            .Setup(service =>
                service.UpdateCoinsMarketData(It.IsAny<IEnumerable<CoinMarketDataUpdateRequest>>())
            )
            .ReturnsAsync(failureResult);

        // Act
        var result = await _testedController.UpdateMarketData(requests);

        // Assert
        result
            .Should()
            .BeOfType<NotFoundObjectResult>()
            .Which.Value.Should()
            .BeOfType<ProblemDetails>()
            .Which.Detail.Should()
            .Contain(errorMessage);

        _mockCoinsService.Verify(service => service.UpdateCoinsMarketData(requests), Times.Once);
    }

    [Fact]
    public async Task ReplaceTradingPairs_OnSuccess_CallsServiceAndReturnsOk()
    {
        // Arrange
        var requests = _fixture.CreateMany<TradingPairCreationRequest>(3);
        var coinsWithNewPairs = _fixture.CreateMany<Coin>(3);
        var successResult = Result.Ok(coinsWithNewPairs);

        _mockCoinsService
            .Setup(service =>
                service.ReplaceAllTradingPairs(It.IsAny<IEnumerable<TradingPairCreationRequest>>())
            )
            .ReturnsAsync(successResult);

        // Act
        var result = await _testedController.ReplaceTradingPairs(requests);

        // Assert
        result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(coinsWithNewPairs);

        _mockCoinsService.Verify(service => service.ReplaceAllTradingPairs(requests), Times.Once);
    }

    [Fact]
    public async Task ReplaceTradingPairs_OnFailure_CallsServiceAndReturnsBadRequest()
    {
        // Arrange
        var requests = _fixture.CreateMany<TradingPairCreationRequest>(3);
        var errorMessage = "Invalid trading pair data";
        var failureResult = Result.Fail<IEnumerable<Coin>>(
            new GenericErrors.BadRequestError(errorMessage)
        );

        _mockCoinsService
            .Setup(service =>
                service.ReplaceAllTradingPairs(It.IsAny<IEnumerable<TradingPairCreationRequest>>())
            )
            .ReturnsAsync(failureResult);

        // Act
        var result = await _testedController.ReplaceTradingPairs(requests);

        // Assert
        result
            .Should()
            .BeOfType<BadRequestObjectResult>()
            .Which.Value.Should()
            .BeOfType<ProblemDetails>()
            .Which.Detail.Should()
            .Contain(errorMessage);

        _mockCoinsService.Verify(service => service.ReplaceAllTradingPairs(requests), Times.Once);
    }

    [Fact]
    public async Task DeleteCoin_OnSuccess_CallsServiceAndReturnsNoContent()
    {
        // Arrange
        var idCoin = _fixture.Create<int>();
        var successResult = Result.Ok();

        _mockCoinsService
            .Setup(service => service.DeleteMainCoin(It.IsAny<int>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _testedController.DeleteMainCoin(idCoin);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mockCoinsService.Verify(service => service.DeleteMainCoin(idCoin), Times.Once);
    }

    [Fact]
    public async Task DeleteCoin_OnNotFound_CallsServiceAndReturnsNotFound()
    {
        // Arrange
        var idCoin = _fixture.Create<int>();
        var errorMessage = $"Coin with ID {idCoin} not found.";
        var failureResult = Result.Fail(new GenericErrors.NotFoundError(errorMessage));

        _mockCoinsService
            .Setup(service => service.DeleteMainCoin(It.IsAny<int>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _testedController.DeleteMainCoin(idCoin);

        // Assert
        result
            .Should()
            .BeOfType<NotFoundObjectResult>()
            .Which.Value.Should()
            .BeOfType<ProblemDetails>()
            .Which.Detail.Should()
            .Contain(errorMessage);

        _mockCoinsService.Verify(service => service.DeleteMainCoin(idCoin), Times.Once);
    }

    [Fact]
    public async Task DeleteUnreferencedCoins_CallsServiceAndReturnsNoContent()
    {
        // Arrange
        _mockCoinsService
            .Setup(service => service.DeleteCoinsNotReferencedByTradingPairs())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _testedController.DeleteUnreferencedCoins();

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mockCoinsService.Verify(
            service => service.DeleteCoinsNotReferencedByTradingPairs(),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteAllCoins_CallsServiceAndReturnsNoContent()
    {
        // Arrange
        _mockCoinsService
            .Setup(service => service.DeleteAllCoinsWithRelatedData())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _testedController.DeleteAllCoins();

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mockCoinsService.Verify(service => service.DeleteAllCoinsWithRelatedData(), Times.Once);
    }
}
