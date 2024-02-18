using Microsoft.EntityFrameworkCore;
using Npgsql;
using Toki.ActivityPub.Models;

namespace Toki.ActivityPub.Persistence.DatabaseContexts;

/// <summary>
/// The Toki DbContext.
/// </summary>
public class TokiDatabaseContext : DbContext
{
    /// <summary>
    /// The users.
    /// </summary>
    public DbSet<User> Users { get; private set; } = null!;
    
    /// <summary>
    /// The posts.
    /// </summary>
    public DbSet<Post> Posts { get; private set; } = null!;
    
    /// <summary>
    /// The follower relations.
    /// </summary>
    public DbSet<FollowerRelation> FollowerRelations { get; private set; } = null!;
    
    /// <summary>
    /// The keypairs.
    /// </summary>
    public DbSet<Keypair> Keypairs { get; private set; } = null!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .OwnsOne<Keypair>(u => u.Keypair);
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=Toki;Username=toki;Password=toki");
    }
}