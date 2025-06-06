using System.Data;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SVC_Coins.Domain.Entities;
using SVC_Coins.Domain.ValueObjects;
using SVC_Coins.Infrastructure;
using SVC_Coins.Repositories.Interfaces;

namespace SVC_Coins.Repositories;

/// <summary>
/// Repository for handling operations related to coins using Entity Framework.
/// </summary>
/// <param name="context">The DbContext used for database operations.</param>
public class CoinsRepository(CoinsDbContext context) : ICoinsRepository
{
    private readonly CoinsDbContext _context = context;

    /// <inheritdoc />
    public async Task<IEnumerable<CoinsEntity>> GetAllCoinsWithRelations() =>
        await _context
            .Coins.Include(c => c.TradingPairs)
            .ThenInclude(tp => tp.Exchanges)
            .ToListAsync();

    /// <inheritdoc />
    public async Task<IEnumerable<CoinsEntity>> GetCoinsByIds(IEnumerable<int> ids) =>
        await _context.Coins.Where(coin => ids.Contains(coin.Id)).ToListAsync();

    /// <inheritdoc />
    public async Task<IEnumerable<CoinsEntity>> GetCoinsByIdsWithRelations(IEnumerable<int> ids) =>
        await _context
            .Coins.Where(coin => ids.Contains(coin.Id))
            .Include(c => c.TradingPairs)
            .ThenInclude(tp => tp.CoinQuote)
            .Include(c => c.TradingPairs)
            .ThenInclude(tp => tp.Exchanges)
            .AsSplitQuery()
            .ToListAsync();

    /// <inheritdoc />
    public async Task<IEnumerable<CoinsEntity>> GetCoinsBySymbolNamePairs(
        IEnumerable<CoinSymbolNamePair> pairs
    )
    {
        var coins = pairs.Select(Mapping.ToCoinsEntity).ToList();

        // Add UseTempDB if needed
        var bulkConfig = new BulkConfig
        {
            UpdateByProperties = [nameof(CoinsEntity.Symbol), nameof(CoinsEntity.Name)],
        };

        await _context.BulkReadAsync(coins, bulkConfig);

        var foundCoins = coins.Where(coin => coin.Id != 0).ToList();

        return foundCoins;
    }

    public async Task<HashSet<int>> GetMissingCoinIds(HashSet<int> coinIds)
    {
        var existingIds = await GetExistingCoinIds(coinIds);

        var missingIds = new HashSet<int>(coinIds);
        missingIds.ExceptWith(existingIds);
        return missingIds;
    }

    private async Task<IEnumerable<int>> GetExistingCoinIds(HashSet<int> coinIds) =>
        await _context
            .Coins.AsNoTracking()
            .Where(coin => coinIds.Contains(coin.Id))
            .Select(coin => coin.Id)
            .ToListAsync();

    /// <inheritdoc />
    public async Task<bool> CheckCoinExists(int coinId) =>
        await _context.Coins.AnyAsync(coin => coin.Id == coinId);

    /// <inheritdoc />
    public async Task<IEnumerable<CoinsEntity>> InsertCoins(IEnumerable<CoinsEntity> coins)
    {
        var newCoins = coins.ToList();
        await _context.Coins.AddRangeAsync(newCoins);
        await _context.SaveChangesAsync();
        return newCoins;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CoinsEntity>> UpdateCoins(IEnumerable<CoinsEntity> coins)
    {
        _context.Coins.UpdateRange(coins);
        await _context.SaveChangesAsync();
        return coins;
    }

    /// <inheritdoc />
    public async Task DeleteCoinsNotReferencedByTradingPairs() =>
        await _context
            .Coins.Where(coin =>
                !_context.TradingPairs.Any(tp =>
                    tp.CoinMain.Id == coin.Id || tp.CoinQuote.Id == coin.Id
                )
            )
            .ExecuteDeleteAsync();

    /// <inheritdoc />
    public async Task DeleteAllCoinsWithRelations()
    {
        _context.TradingPairs.RemoveRange(_context.TradingPairs);
        _context.Coins.RemoveRange(_context.Coins);
        await _context.SaveChangesAsync();
    }

    private static class Mapping
    {
        public static CoinsEntity ToCoinsEntity(CoinSymbolNamePair pair) =>
            new() { Symbol = pair.Symbol, Name = pair.Name };
    }
}
