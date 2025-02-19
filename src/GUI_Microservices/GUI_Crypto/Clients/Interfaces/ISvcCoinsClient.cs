using FluentResults;
using GUI_Crypto.Models.Input;
using GUI_Crypto.Models.Output;

namespace GUI_Crypto.Clients.Interfaces;

/// <summary>
/// Interface for the interractions with SVC_Coins microservice.
/// </summary>
public interface ISvcCoinsClient
{
    /// <summary>
    /// Creates a new coin.
    /// </summary>
    /// <param name="coin">The coin to create.</param>
    /// <returns>True if the coin was created successfully,
    /// false if it already exists in the database.</returns>
    Task<bool> CreateCoin(CoinNew coin);

    /// <summary>
    /// Creates multiple new coins in a batch.
    /// </summary>
    /// <param name="coins">The collection of coins to create.</param>
    /// <returns>Success result if all coins were created successfully,
    /// failure result with error message if any coin already exists in the database.</returns>
    Task<Result> CreateCoins(IEnumerable<CoinNew> coins);

    /// <summary>
    /// Retrieves all coins.
    /// </summary>
    /// <returns>A list of all coins.</returns>
    Task<IEnumerable<Coin>> GetAllCoins();

    /// <summary>
    /// Deletes a coin.
    /// </summary>
    /// <param name="idCoin">The ID of the coin to delete.</param>
    /// <returns>A task that deletes the coin.</returns>
    Task DeleteCoin(int idCoin);

    /// <summary>
    /// Creates a new trading pair.
    /// </summary>
    /// <param name="tradingPair">The trading pair to create.</param>
    /// <returns>Id of the created trading pair.</returns>
    Task<int> CreateTradingPair(TradingPairNew tradingPair);

    /// <summary>
    /// Retrieves coins by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of the coins to retrieve.</param>
    /// <returns>A list of coins.</returns>
    Task<IEnumerable<Coin>> GetCoinsByIds(IEnumerable<int> ids);

    /// <summary>
    /// Retrieves quote coins sorted by priority.
    /// </summary>
    /// <returns>A list of quote coins sorted by priority.</returns>
    Task<IEnumerable<Coin>> GetQuoteCoinsPrioritized();
}
