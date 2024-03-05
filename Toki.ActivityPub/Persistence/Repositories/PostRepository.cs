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
    UserRepository userRepo,
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
    /// Finds a like by the id pair.
    /// </summary>
    /// <param name="postId">The post id.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>The post, if it exists.</returns>
    public async Task<PostLike?> FindLikeByIds(
        Guid postId,
        Guid userId)
    {
        return await db.PostLikes
            .FirstOrDefaultAsync(like => like.LikingUserId == userId &&
                                         like.PostId == postId);
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
    /// Creates a custom query.
    /// </summary>
    /// <returns>The query.</returns>
    public IQueryable<Post> CreateCustomQuery() =>
        db.Posts;

    /// <summary>
    /// Imports attachments for a given post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <param name="attachments">The attachments.</param>
    public async Task ImportAttachmentsForNote(
        Post post,
        IEnumerable<ASDocument> attachments)
    {
        foreach (var document in attachments)
        {
            if (document.Url is null)
                continue;
            
            var attachment = new PostAttachment()
            {
                Id = Guid.NewGuid(),

                Parent = post,
                ParentId = post.Id,

                Description = document.Name,
                Url = document.Url
            };
            
            db.PostAttachments.Add(attachment);
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Collects the mentions for a note.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <returns>The mentions, if any exist.</returns>
    private async Task<List<Guid>?> CollectMentions(
        ASNote note)
    {
        if (note.Tags is null)
            return null;
        
        var asMentions = note.Tags
            .OfType<ASMention>()
            .ToList();

        var mentions = new List<Guid>();
        foreach (var mention in asMentions)
        {
            var user = await userRepo.FindByRemoteId(mention.Href);
            if (user is null)
            {
                // TODO: Fetch this user.
                continue;
            }
            
            mentions.Add(user.Id);
        }

        return mentions;
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
            
            Visibility = note.GetPostVisibility(author),
            
            Mentions = await CollectMentions(note)
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

        if (note.Attachments is not null &&
            note.Attachments.Count > 0)
        {
            await ImportAttachmentsForNote(
                post,
                note.Attachments);
        }
        
        return post;
    }
}