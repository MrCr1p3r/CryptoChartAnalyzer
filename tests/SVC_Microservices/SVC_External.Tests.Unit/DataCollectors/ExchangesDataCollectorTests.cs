using AutoFixture;
using FluentAssertions;
using Moq;
using SVC_External.Clients.Interfaces;
using SVC_External.DataCollectors;
using SVC_External.Models.Input;
using SVC_External.Models.Output;

namespace SVC_External.Tests.Unit.DataCollectors;

public class ExchangesDataCollectorTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IExchangeClient> _firstClientMock;
    private readonly Mock<IExchangeClient> _secondClientMock;
    private readonly ExchangesDataCollector _dataCollector;

    public ExchangesDataCollectorTests()
    {
        _fixture = new Fixture();
        _firstClientMock = new Mock<IExchangeClient>();
        _secondClientMock = new Mock<IExchangeClient>();

        var clients = new[] { _firstClientMock.Object, _secondClientMock.Object };
        _dataCollector = new ExchangesDataCollector(clients);
    }

    [Fact]
    public async Task GetKlineData_FirstClientReturnsData_FirstClientCalled()
    {
        // Arrange
        var request = _fixture.Create<KlineDataRequest>();
        var formattedRequest = Mapping.ToFormattedRequest(request);

        var expectedData = _fixture.CreateMany<KlineData>(5);
        _firstClientMock.Setup(c => c.GetKlineData(It.IsAny<KlineDataRequestFormatted>())).ReturnsAsync(expectedData);

        // Act
        await _dataCollector.GetKlineData(request);

        // Assert
        _firstClientMock.Verify(client => client.GetKlineData(formattedRequest), Times.Once);
        _secondClientMock.Verify(client => client.GetKlineData(It.IsAny<KlineDataRequestFormatted>()), Times.Never);
    }

    [Fact]
    public async Task GetKlineData_FirstClientReturnsData_ReturnsExpectedData()
    {
        // Arrange
        var request = _fixture.Create<KlineDataRequest>();
        var formattedRequest = Mapping.ToFormattedRequest(request);

        var expectedData = _fixture.CreateMany<KlineData>(5);
        _firstClientMock.Setup(c => c.GetKlineData(It.IsAny<KlineDataRequestFormatted>())).ReturnsAsync(expectedData);
        _secondClientMock.Setup(c => c.GetKlineData(It.IsAny<KlineDataRequestFormatted>())).ReturnsAsync([]);

        // Act
        var result = await _dataCollector.GetKlineData(request);

        // Assert
        result.Should().BeEquivalentTo(expectedData);
    }

    [Fact]
    public async Task GetKlineData_FirstClientReturnsNoData_VerifiesBothClientsCalled()
    {
        // Arrange
        var request = _fixture.Create<KlineDataRequest>();
        var formattedRequest = Mapping.ToFormattedRequest(request);

        _firstClientMock.Setup(c => c.GetKlineData(It.IsAny<KlineDataRequestFormatted>())).ReturnsAsync([]);
        _secondClientMock.Setup(c => c.GetKlineData(It.IsAny<KlineDataRequestFormatted>()))
            .ReturnsAsync(_fixture.CreateMany<KlineData>(3));

        // Act
        await _dataCollector.GetKlineData(request);

        // Assert
        _firstClientMock.Verify(c => c.GetKlineData(formattedRequest), Times.Once);
        _secondClientMock.Verify(c => c.GetKlineData(formattedRequest), Times.Once);
    }

    [Fact]
    public async Task GetKlineData_FirstClientReturnsNoData_ReturnsDataFromSecondClient()
    {
        // Arrange
        var request = _fixture.Create<KlineDataRequest>();
        var formattedRequest = Mapping.ToFormattedRequest(request);

        var expectedData = _fixture.CreateMany<KlineData>(3);
        _firstClientMock.Setup(c => c.GetKlineData(It.IsAny<KlineDataRequestFormatted>())).ReturnsAsync([]);
        _secondClientMock.Setup(c => c.GetKlineData(It.IsAny<KlineDataRequestFormatted>())).ReturnsAsync(expectedData);

        // Act
        var result = await _dataCollector.GetKlineData(request);

        // Assert
        result.Should().BeEquivalentTo(expectedData);
    }

    [Fact]
    public async Task GetKlineData_NoClientReturnsData_ReturnsEmptyCollection()
    {
        // Arrange
        var request = _fixture.Create<KlineDataRequest>();
        var formattedRequest = Mapping.ToFormattedRequest(request);

        _firstClientMock.Setup(c => c.GetKlineData(It.IsAny<KlineDataRequestFormatted>())).ReturnsAsync([]);
        _secondClientMock.Setup(c => c.GetKlineData(It.IsAny<KlineDataRequestFormatted>())).ReturnsAsync([]);

        // Act
        var result = await _dataCollector.GetKlineData(request);

        // Assert
        result.Should().BeEmpty();
    }

    private static class Mapping
    {
        public static KlineDataRequestFormatted ToFormattedRequest(KlineDataRequest request) => new()
        {
            CoinMain = request.CoinMain,
            CoinQuote = request.CoinQuote,
            Interval = request.Interval,
            StartTimeUnix = new DateTimeOffset(request.StartTime).ToUnixTimeMilliseconds(),
            EndTimeUnix = new DateTimeOffset(request.EndTime).ToUnixTimeMilliseconds(),
            Limit = request.Limit
        };
    }
}
