using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// The post repository.
/// </summary>
/// <param name="db">The database.</param>
public class PostRepository(
    TokiDatabaseContext db,
    IOptions<InstanceConfiguration> opts)
{
    /// <summary>
    /// Finds a post by its remote id.
    /// </summary>
    /// <param name="remoteId">The remote id of the post.</param>
    /// <returns>The post if it exists.</returns>
    public async Task<Post?> FindByRemoteId(string remoteId)
    {
        var post = await db.Posts
            .Include(post => post.Author)
            .FirstOrDefaultAsync(post => post.RemoteId == remoteId);
        
        // TODO: I honestly don't know whether this is the best idea but whatever.
        if (post is null &&
            remoteId.StartsWith($"https://{opts.Value.Domain}/posts"))
        {
            var handle = remoteId.Split('/')
                .Last();
            
            if (Guid.TryParse(handle, out var id))
                return await FindById(id);
        }

        return post;
    }
    
    /// <summary>
    /// Finds a post by its id.
    /// </summary>
    /// <param name="id">The id of the post.</param>
    /// <returns>The post if it exists.</returns>
    public async Task<Post?> FindById(Guid id)
    {
        return await db.Posts
            .Include(post => post.Author)
            .FirstOrDefaultAsync(post => post.Id == id);
    }

    /// <summary>
    /// Adds a post to the database.
    /// </summary>
    /// <param name="post">The post.</param>
    public async Task Add(Post post)
    {
        db.Posts.Add(post);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Adds a like to the post likes collection.
    /// </summary>
    /// <param name="like">The like.</param>
    /// <param name="post">The post.</param>
    public async Task AddLike(
        PostLike like,
        Post post)
    {
        // Increment the amount of likes for this post.
        post.LikeCount++;
        db.Posts.Update(post);
        
        db.PostLikes.Add(like);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Adds a boost to the post.
    /// </summary>
    /// <param name="boost">The boost.</param>
    /// <param name="post">The post.</param>
    public async Task AddBoost(
        Post boost,
        Post post)
    {
        // Increment the amount of likes for this post.
        post.BoostCount++;
        db.Posts.Update(post);
        
        db.Posts.Add(boost);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Imports an ActivityStreams note as a post.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <param name="author">The actor who was the author.</param>
    /// <returns>The post, if the import was successful.</returns>
    public async Task<Post?> ImportFromActivityStreams(
        ASNote note,
        User author)
    {
        var post = new Post()
        {
            Id = Guid.NewGuid(),
            RemoteId = note.Id,
            
            Author = author,
            AuthorId = author.Id,

            Content = note.Content!,
            
            Sensitive = note.Sensitive ?? false,
            
            CreatedAt = note.PublishedAt?
                .ToUniversalTime() ?? DateTimeOffset.UtcNow,
            
            Visibility = note.GetPostVisibility(author)
        };
        
        // TODO: Resolve parent post chain.
        if (note.InReplyTo is not null)
        {
            var parent = await FindByRemoteId(note.InReplyTo.Id);
            post.Parent = parent;
        }
        
        // TODO: Resolve quote posts.
        if (note.Quoting is not null)
        {
            var quoting = await FindByRemoteId(note.Quoting.Id);
        }

        await Add(post);
        return post;
    }
}