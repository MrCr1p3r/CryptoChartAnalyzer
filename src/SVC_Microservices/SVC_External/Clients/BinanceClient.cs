using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharedLibrary.Enums;
using SharedLibrary.Extensions;
using SVC_External.Clients.Interfaces;
using SVC_External.Models.Input;
using SVC_External.Models.Output;

namespace SVC_External.Clients;

/// <summary>
/// Implements methods for interracting with Binance API.
/// </summary>
public class BinanceClient(IHttpClientFactory httpClientFactory, ILogger<BinanceClient> logger)
    : IExchangeClient
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("BinanceClient");
    private readonly ILogger<BinanceClient> _logger = logger;

    /// <inheritdoc />
    public async Task<IEnumerable<KlineData>> GetKlineData(KlineDataRequestFormatted request)
    {
        var endpoint = Mapping.ToBinanceKlineEndpoint(request);
        var httpResponse = await _httpClient.GetAsync(endpoint);
        if (!httpResponse.IsSuccessStatusCode)
        {
            await _logger.LogUnsuccessfulHttpResponse(httpResponse);
            return [];
        }

        var rawData = await httpResponse.Content.ReadFromJsonAsync<List<List<JsonElement>>>();
        return rawData!.Select(Mapping.ToKlineData);
    }

    /// <inheritdoc />
    public async Task<ListedCoins> GetAllListedCoins(ListedCoins listedCoins)
    {
        var endpoint = "/api/v3/exchangeInfo";
        var httpResponse = await _httpClient.GetAsync(endpoint);
        if (!httpResponse.IsSuccessStatusCode)
        {
            await _logger.LogUnsuccessfulHttpResponse(httpResponse);
            return listedCoins;
        }

        var binanceResponse =
            await httpResponse.Content.ReadFromJsonAsync<ResponseDtos.BinanceResponse>();
        listedCoins.BinanceCoins = binanceResponse!.Symbols.Select(symbol => symbol.BaseAsset);
        return listedCoins;
    }

    private static class ResponseDtos
    {
        public class BinanceResponse
        {
            [JsonPropertyName("symbols")]
            public HashSet<BinanceSymbol> Symbols { get; set; } = [];
        }

        public record BinanceSymbol
        {
            [JsonPropertyName("baseAsset")]
            public string BaseAsset { get; set; } = string.Empty;
        }
    }

    private static class Mapping
    {
        public static string ToBinanceKlineEndpoint(KlineDataRequestFormatted request) =>
            $"/api/v3/klines?symbol={request.CoinMain + request.CoinQuote}"
            + $"&interval={ToBinanceTimeFrame(request.Interval)}"
            + $"&limit={request.Limit}"
            + $"&startTime={request.StartTimeUnix}"
            + $"&endTime={request.EndTimeUnix}";

        public static string ToBinanceTimeFrame(ExchangeKlineInterval timeFrame) =>
            timeFrame switch
            {
                ExchangeKlineInterval.OneMinute => "1m",
                ExchangeKlineInterval.FiveMinutes => "5m",
                ExchangeKlineInterval.FifteenMinutes => "15m",
                ExchangeKlineInterval.ThirtyMinutes => "30m",
                ExchangeKlineInterval.OneHour => "1h",
                ExchangeKlineInterval.FourHours => "4h",
                ExchangeKlineInterval.OneDay => "1d",
                ExchangeKlineInterval.OneWeek => "1w",
                ExchangeKlineInterval.OneMonth => "1M",
                _ => throw new ArgumentException($"Unsupported TimeFrame: {timeFrame}"),
            };

        public static KlineData ToKlineData(List<JsonElement> data)
        {
            var ic = CultureInfo.InvariantCulture;
            return new()
            {
                OpenTime = data[0].GetInt64(),
                OpenPrice = Convert.ToDecimal(data[1].GetString(), ic),
                HighPrice = Convert.ToDecimal(data[2].GetString(), ic),
                LowPrice = Convert.ToDecimal(data[3].GetString(), ic),
                ClosePrice = Convert.ToDecimal(data[4].GetString(), ic),
                Volume = Convert.ToDecimal(data[5].GetString(), ic),
                CloseTime = data[6].GetInt64(),
            };
        }
    }
}
