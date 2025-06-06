namespace SVC_Kline.ApiContracts.Requests;

/// <summary>
/// Represents Kline (candlestick) data for a trading pair for a new entry in the db.
/// </summary>
public class KlineDataCreationRequest
{
    /// <summary>
    /// Id of the trading pair for which the Kline data is recorded.
    /// </summary>
    public int IdTradingPair { get; set; }

    /// <summary>
    /// The opening time of the Kline in milliseconds since the Unix epoch.
    /// </summary>
    public long OpenTime { get; set; }

    /// <summary>
    /// The opening price at the start of the Kline period.
    /// </summary>
    public decimal OpenPrice { get; set; }

    /// <summary>
    /// The highest price during the Kline period.
    /// </summary>
    public decimal HighPrice { get; set; }

    /// <summary>
    /// The lowest price during the Kline period.
    /// </summary>
    public decimal LowPrice { get; set; }

    /// <summary>
    /// The closing price at the end of the Kline period.
    /// </summary>
    public decimal ClosePrice { get; set; }

    /// <summary>
    /// The total volume traded during the Kline period.
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// The closing time of the Kline in milliseconds since the Unix epoch.
    /// </summary>
    public long CloseTime { get; set; }
}
