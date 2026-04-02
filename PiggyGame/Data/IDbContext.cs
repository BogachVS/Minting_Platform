using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PiggyGame.Data.Entities;

namespace PiggyGame.Data;

/// <summary>
/// Main DbContext that contains sets of all entities.
/// Entities referenced from <c>Domain</c> project.
/// </summary>
public interface IDbContext : IDisposable
{
    DbSet<TreasuryBalance> TreasuryBalances => Set<TreasuryBalance>();
    DbSet<InternalCallbackData> InternalCallbackData => Set<InternalCallbackData>();
    DbSet<User> Users => Set<User>();
    DbSet<Game> Games => Set<Game>();
    DbSet<AuthCode> AuthCodes => Set<AuthCode>();

    // Database
    public DatabaseFacade DatabaseFacade { get; }

    // Helpers
    protected DbSet<TEntity> Set<TEntity>() where TEntity : Entity;

    public int SaveChanges();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
