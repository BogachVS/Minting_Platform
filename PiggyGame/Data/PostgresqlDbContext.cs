using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PiggyGame.Data.Entities;

namespace PiggyGame.Data;

public sealed class PostgresqlDbContext : DbContext, IDbContext
{
    public DatabaseFacade DatabaseFacade => Database;
    
    // Constructors.
    public PostgresqlDbContext(DbContextOptions<PostgresqlDbContext> opt) : base(opt) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Load all entities configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresqlDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
    
    // Methods.
    public new DbSet<TEntity> Set<TEntity>() where TEntity : Entity
    {
        return base.Set<TEntity>();
    }

    public new int SaveChanges()
    {
        UpdateEntitiesDateProperties();
        
        return base.SaveChanges();
    }

    public new Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateEntitiesDateProperties();
        
        return base.SaveChangesAsync(cancellationToken);
    }
    
    // Helpers.
    // Sets entity's "UpdatedAt" and "CreatedAt" properties to current UTC datetime.
    private void UpdateEntitiesDateProperties()
    {
        // Get all added or modified entities
        var entries = ChangeTracker
            .Entries()
            .Where(x => x is { Entity: Entity, State: EntityState.Added or EntityState.Modified });

        foreach (var entityEntry in entries)
        {
            var entity = (Entity)entityEntry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;
            
            // Do not replace the creation date if it exists.
            // This occurs if the entity has been modified or its creation date has been set manually.
            if (entity.CreatedAt == default)
            {
                entity.CreatedAt = DateTime.UtcNow;   
            }
        }
    }
}