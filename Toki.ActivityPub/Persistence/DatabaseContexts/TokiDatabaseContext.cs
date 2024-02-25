using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;

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

    /// <summary>
    /// The instances set.
    /// </summary>
    public DbSet<RemoteInstance> Instances { get; private set; } = null!;
    
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

        modelBuilder.Entity<User>()
            .HasOne<RemoteInstance>(u => u.ParentInstance)
            .WithMany()
            .HasForeignKey(u => u.ParentInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=Toki;Username=toki;Password=toki;Include Error Detail=True");
    }
}