using Microsoft.EntityFrameworkCore;
using SVC_Kline.Models.Entities;

namespace SVC_Kline.Repositories;

/// <summary>
/// The DbContext class for managing database operations related to Kline data.
/// </summary>
/// <param name="options">The options to configure the DbContext.</param>
public class KlineDataDbContext(DbContextOptions<KlineDataDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the DbSet for Kline data entities.
    /// </summary>
    public DbSet<KlineDataEntity> KlineData { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for trading pair entities.
    /// </summary>
    public DbSet<TradingPairEntity> TradingPair { get; set; } = null!;

    /// <summary>
    /// Configures the model and relationships using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<KlineDataEntity>(entity =>
        {
            entity
                .HasKey(e => new { e.IdTradePair, e.OpenTime })
                .HasName("PK__KlineDat__031B42F9984E192C");

            entity.Property(e => e.ClosePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HighPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LowPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OpenPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Volume).HasColumnType("decimal(18, 2)");

            entity
                .HasOne(d => d.IdTradePairNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdTradePair)
                .HasConstraintName("FK_KlineData_TradingPairs");
        });

        modelBuilder.Entity<TradingPairEntity>().ToTable("TradingPair").HasKey(e => e.Id);
    }
}
