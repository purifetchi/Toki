using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Posts;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityPub.Renderers;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// The post repository.
/// </summary>
/// <param name="db">The database.</param>
public class PostRepository(
    TokiDatabaseContext db,
    UserRepository userRepo,
    IOptions<InstanceConfiguration> opts,
    InstancePathRenderer pathRenderer)
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
            
            if (Ulid.TryParse(handle, out var id))
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
        Ulid postId,
        Ulid userId)
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
    public async Task<Post?> FindById(Ulid id)
    {
        return await db.Posts
            .Include(post => post.Attachments)
            .Include(post => post.Author)
            .Include(post => post.Parent)
            .ThenInclude(parent => parent!.Author)
            .FirstOrDefaultAsync(post => post.Id == id);
    }

    /// <summary>
    /// Finds a boost by its id and author.
    /// </summary>
    /// <param name="author">The author.</param>
    /// <param name="id">The id of the boosted post.</param>
    /// <returns>The boost, if it exists.</returns>
    public async Task<Post?> FindBoostByIdAndAuthor(
        User author,
        Ulid id)
    {
        return await db.Posts
            .Include(post => post.Boosting)
            .Include(post => post.Boosting!.Author)
            .Include(post => post.Boosting!.Attachments)
            .FirstOrDefaultAsync(post => post.BoostingId == id && post.AuthorId == author.Id);
    }

    /// <summary>
    /// Adds a post to the database.
    /// </summary>
    /// <param name="post">The post.</param>
    public async Task Add(Post post)
    {
        db.Posts.Add(post);

        // Update the post count.
        post.Author.PostCount++;
        db.Users.Update(post.Author);
        
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
    /// Removes a like from a post.
    /// </summary>
    /// <param name="like">The like.</param>
    public async Task RemoveLike(
        PostLike like)
    {
        var post = like.Post;
        post.LikeCount--;
        db.Posts.Update(post);
        
        db.PostLikes.Remove(like);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes a boost from a post.
    /// </summary>
    /// <param name="boost">The boost.</param>
    public async Task RemoveBoost(
        Post boost)
    {
        if (boost.Boosting is null)
            return;
        
        var post = boost.Boosting!;
        post.BoostCount--;
        db.Posts.Update(post);
        
        db.Posts.Remove(boost);
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
    /// Creates a custom query for post likes.
    /// </summary>
    /// <returns>The post likes query.</returns>
    public IQueryable<PostLike> CreateCustomLikeQuery() =>
        db.PostLikes;
    
    /// <summary>
    /// Creates a detached attachment (one without a parent <see cref="Post"/>).
    /// </summary>
    /// <param name="url">The url of the file.</param>
    /// <param name="description">Its description.</param>
    /// <param name="mime">The mime type of the file.</param>
    /// <returns>The attachment.</returns>
    public async Task<PostAttachment> CreateDetachedAttachment(
        string url,
        string? description,
        string? mime)
    {
        var attachment = new PostAttachment
        {
            Id = Ulid.NewUlid(),
            Url = url,
            Description = description,
            Mime = mime
        };

        db.PostAttachments.Add(attachment);
        await db.SaveChangesAsync();

        return attachment;
    }

    /// <summary>
    /// Returns multiple attachments by their ids.
    /// </summary>
    /// <param name="ids">The id list.</param>
    /// <returns>The list of attachments.</returns>
    public async Task<IList<PostAttachment>?> FindMultipleAttachmentsByIds(
        IList<Ulid> ids)
    {
        return await db.PostAttachments
            .Where(pa => ids.Contains(pa.Id))
            .ToListAsync();
    }

    /// <summary>
    /// Finds an attachment by its id.
    /// </summary>
    /// <param name="id">The id of the attachment.</param>
    /// <returns>The attachment, or nothing.</returns>
    public async Task<PostAttachment?> FindAttachmentById(
        Ulid id)
    {
        return await db.PostAttachments
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Updates an attachment
    /// </summary>
    /// <param name="attachment">The attachment.</param>
    public async Task UpdateAttachment(
        PostAttachment attachment)
    {
        db.PostAttachments.Update(attachment);
        await db.SaveChangesAsync();
    }

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
                Id = Ulid.NewUlid(),

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
    private async Task<List<PostMention>?> CollectMentions(
        ASNote note)
    {
        if (note.Tags is null)
            return null;
        
        var asMentions = note.Tags
            .OfType<ASMention>()
            .ToList();

        var mentions = new List<PostMention>();
        foreach (var mention in asMentions)
        {
            var user = await userRepo.FindByRemoteId(mention.Href);
            if (user is null)
            {
                // TODO: Fetch this user.
                continue;
            }
            
            mentions.Add(new PostMention
            {
                Id = user.Id.ToString(),
                Handle = user.Handle,
                Url = user.RemoteId ?? pathRenderer.GetPathToActor(user)
            });
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
            Id = Ulid.NewUlid(),
            RemoteId = note.Id,
            
            Context = Guid.NewGuid(),
            
            Author = author,
            AuthorId = author.Id,

            Content = note.Content!,
            
            Sensitive = note.Sensitive ?? false,
            
            CreatedAt = note.PublishedAt?
                .ToUniversalTime() ?? DateTimeOffset.UtcNow,
            
            Visibility = note.GetPostVisibility(author),
            
            UserMentions = await CollectMentions(note)
        };
        
        if (note.InReplyTo is not null)
        {
            var parent = await FindByRemoteId(note.InReplyTo.Id);
            if (parent is not null)
            {
                post.Parent = parent;
                post.Context = parent.Context;    
            }
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