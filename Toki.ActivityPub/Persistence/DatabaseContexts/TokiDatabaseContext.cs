using Microsoft.EntityFrameworkCore;
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

    /// <summary>
    /// The credentials.
    /// </summary>
    public DbSet<Credentials> Credentials { get; private set; } = null!;
    
    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne<Keypair>(u => u.Keypair)
            .WithOne(k => k.Owner)
            .IsRequired()
            .HasForeignKey<Keypair>(k => k.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FollowerRelation>()
            .HasOne(fr => fr.Followee)
            .WithOne()
            .IsRequired()
            .HasForeignKey<FollowerRelation>(fr => fr.FolloweeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FollowerRelation>()
            .HasOne(fr => fr.Follower)
            .WithOne()
            .IsRequired()
            .HasForeignKey<FollowerRelation>(fr => fr.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Credentials>()
            .HasOne<User>(c => c.User)
            .WithOne()
            .HasForeignKey<Credentials>(c => c.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=Toki;Username=toki;Password=toki");
    }
}