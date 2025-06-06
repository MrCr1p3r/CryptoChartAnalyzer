using SVC_Coins.Domain.Entities;
using SVC_Coins.Domain.ValueObjects;

namespace SVC_Coins.Repositories.Interfaces;

/// <summary>
/// Interface for the repository that handles operations related to Coins.
/// </summary>
public interface ICoinsRepository
{
    /// <summary>
    /// Retrieves all coins with their related data from the database.
    /// </summary>
    /// <returns>A collection of retrieved coin entities.</returns>
    Task<IEnumerable<CoinsEntity>> GetAllCoinsWithRelations();

    /// <summary>
    /// Retrieves a collection of coins by their IDs from the database.
    /// </summary>
    /// <param name="ids">The IDs of the coins to retrieve.</param>
    /// <returns>A collection of found coin entities.</returns>
    Task<IEnumerable<CoinsEntity>> GetCoinsByIds(IEnumerable<int> ids);

    /// <summary>
    /// Retrieves a collection of coins by their IDs with their related data from the database.
    /// </summary>
    /// <param name="ids">The IDs of the coins to retrieve.</param>
    /// <returns>A collection of found coin entities.</returns>
    Task<IEnumerable<CoinsEntity>> GetCoinsByIdsWithRelations(IEnumerable<int> ids);

    /// <summary>
    /// Retrieves a collection of coins that match the specified symbol-name pairs.
    /// </summary>
    /// <param name="pairs">The collection of symbol-name pairs to match.</param>
    /// <returns>A collection of found coin entities.</returns>
    Task<IEnumerable<CoinsEntity>> GetCoinsBySymbolNamePairs(IEnumerable<CoinSymbolNamePair> pairs);

    /// <summary>
    /// Gets the IDs of coins from the provided list that are missing in the database.
    /// </summary>
    /// <param name="coinIds">The collection of coin IDs to check.</param>
    /// <returns>A HashSet containing the IDs of the missing coins.</returns>
    Task<HashSet<int>> GetMissingCoinIds(HashSet<int> coinIds);

    /// <summary>
    /// Checks if a coin with the specified ID exists in the database.
    /// </summary>
    /// <param name="coinId">The ID of the coin to check.</param>
    /// <returns>True if the coin exists, false otherwise.</returns>
    Task<bool> CheckCoinExists(int coinId);

    /// <summary>
    /// Inserts multiple new coin entities into the database.
    /// </summary>
    /// <param name="coins">The collection of coin entities to insert.</param>
    /// <returns>A collection of inserted coin entities.</returns>
    Task<IEnumerable<CoinsEntity>> InsertCoins(IEnumerable<CoinsEntity> coins);

    /// <summary>
    /// Updates the data of multiple coin entities.
    /// </summary>
    /// <param name="coins">The collection of coin entities with updated data.</param>
    /// <returns>A collection of coin entities with updated data.</returns>
    Task<IEnumerable<CoinsEntity>> UpdateCoins(IEnumerable<CoinsEntity> coins);

    /// <summary>
    /// Deletes all coins from the database that are neither referenced as a main
    /// nor a quote coin in any trading pair.
    /// </summary>
    /// <returns>A task, that deletes all coins without trading pairs reference from the database.</returns>
    Task DeleteCoinsNotReferencedByTradingPairs();

    /// <summary>
    /// Removes all coins together with their related data from the database.
    /// </summary>
    /// <returns>Task, that removes all coins together with their related data from the database.</returns>
    Task DeleteAllCoinsWithRelations();
}
