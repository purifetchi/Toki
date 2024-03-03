using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.OAuth;

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
    /// The posts.
    /// </summary>
    public DbSet<PostAttachment> PostAttachments { get; private set; } = null!;
    
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

    /// <summary>
    /// The follow requests set.
    /// </summary>
    public DbSet<FollowRequest> FollowRequests { get; private set; } = null!;

    /// <summary>
    /// The post likes set.
    /// </summary>
    public DbSet<PostLike> PostLikes { get; private set; } = null!;

    /// <summary>
    /// The OAuth2 apps set.
    /// </summary>
    public DbSet<OAuthApp> OAuthApps { get; private set; } = null!;
    
    /// <summary>
    /// The OAuth2 tokens set.
    /// </summary>
    public DbSet<OAuthToken> OAuthTokens { get; private set; } = null!;
    
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
        
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Parent)
            .WithMany()
            .HasForeignKey(p => p.ParentId)
            .OnDelete(DeleteBehavior.SetNull); // If the parent gets deleted, the children just get detached.
        
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Boosting)
            .WithMany()
            .HasForeignKey(p => p.BoostingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FollowerRelation>()
            .HasOne(fr => fr.Followee)
            .WithMany()
            .IsRequired()
            .HasForeignKey(fr => fr.FolloweeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FollowerRelation>()
            .HasOne(fr => fr.Follower)
            .WithMany()
            .IsRequired()
            .HasForeignKey(fr => fr.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<FollowRequest>()
            .HasOne(fr => fr.From)
            .WithMany()
            .IsRequired()
            .HasForeignKey(fr => fr.FromId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FollowRequest>()
            .HasOne(fr => fr.To)
            .WithMany()
            .IsRequired()
            .HasForeignKey(fr => fr.ToId)
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

        modelBuilder.Entity<PostLike>()
            .HasOne<Post>(pl => pl.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<PostLike>()
            .HasOne<User>(pl => pl.LikingUser)
            .WithMany()
            .HasForeignKey(pl => pl.LikingUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostAttachment>()
            .HasOne<Post>(pa => pa.Parent)
            .WithMany(p => p.Attachments)
            .HasForeignKey(pa => pa.ParentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OAuthToken>()
            .HasOne<OAuthApp>(oa => oa.ParentApp)
            .WithMany()
            .HasForeignKey(oa => oa.ParentAppId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<OAuthToken>()
            .HasOne<User>(oa => oa.User)
            .WithMany()
            .HasForeignKey(oa => oa.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=Toki;Username=toki;Password=toki;Include Error Detail=True");
    }
}