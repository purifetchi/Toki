using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Emojis;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Federation;
using Toki.ActivityPub.Formatters;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.DTO;
using Toki.ActivityPub.Models.Posts;
using Toki.ActivityPub.Notifications;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.Users;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Posts;

/// <summary>
/// The class responsible for managing post deletion/creation.
/// </summary>
/// <param name="repo">The post repository.</param>
/// <param name="postRenderer">The post ActivityStreams renderer.</param>
/// <param name="federationService">The federation service.</param>
public class PostManagementService(
    PostRepository repo,
    PostRenderer postRenderer,
    MessageFederationService federationService,
    NotificationService notificationService,
    ContentFormatter formatter,
    InstancePathRenderer pathRenderer,
    UserManagementService userManagementService,
    EmojiService emojiService,
    ActivityPubResolver resolver,
    IHtmlSanitizer htmlSanitizer,
    IOptions<InstanceConfiguration> opts,
    ILogger<PostManagementService> logger)
{
    /// <summary>
    /// Creates a new post.
    /// </summary>
    /// <param name="creationRequest">The creation request.</param>
    /// <returns>The created post.</returns>
    public async Task<Post?> Create(
        PostCreationRequest creationRequest)
    {
        if (creationRequest.Author.IsRemote)
            return null;

        if (creationRequest.Content.Length > opts.Value.Limits.MaxPostCharacterLimit)
            return null;
        
        if (creationRequest.Media?.Count > opts.Value.Limits.MaxAttachmentCount)
            return null;

        var formattingResult = await formatter.Format(
            creationRequest.Content);

        if (formattingResult is null)
            return null;

        var post = new Post()
        {
            Id = Ulid.NewUlid(),
            Context = creationRequest.InReplyTo?.Context ??
                      Guid.NewGuid(),
            
            Author = creationRequest.Author,
            AuthorId = creationRequest.Author.Id,

            Content = formattingResult.Formatted,
            
            Sensitive = creationRequest.IsSensitive,
            ContentWarning = creationRequest.ContentWarning,
            
            Visibility = creationRequest.Visibility,
            
            Parent = creationRequest.InReplyTo,
            ParentId = creationRequest.InReplyTo?.Id,
            
            Attachments = creationRequest.Media,
            
            UserMentions = formattingResult.Mentions
                .Select(u => new PostMention
                {
                    Id = u.Id.ToString(),
                    Handle = u.Handle,
                    Url = u.RemoteId ?? pathRenderer.GetPathToActor(u)
                })
                .ToList(),
            
            Tags = formattingResult.Hashtags,
            Emojis = formattingResult.Emojis
        };

        await repo.Add(post);

        var creation = await postRenderer.RenderCreationForNote(post);
        if (creationRequest.InReplyTo is null)
        {
            await federationService.SendToFollowers(
                creationRequest.Author,
                creation);
        }
        else
        {
            await federationService.SendToRelevantPostUsers(
                creationRequest.Author,
                creationRequest.InReplyTo,
                creation);
        }

        await notificationService.DispatchAllNotificationsForPost(post);
        
        return post;
    }

    /// <summary>
    /// Deletes a post.
    /// </summary>
    /// <param name="post">The post.</param>
    public async Task Delete(Post post)
    {
        await repo.Delete(post);
        
        // TODO: Subtract from the like and boost counts of the users that have boosted
        //       and liked this post.
        
        // Dispatch a removal if this is a local post.
        if (post.RemoteId is not null)
            return;

        // TODO: Send this to every person that's interested in the post.
        await federationService.SendToFollowers(
            post.Author,
            postRenderer.RenderDeletionForNote(post));
    }

    /// <summary>
    /// Boosts a post.
    /// </summary>
    /// <param name="user">The user who's boosting.</param>
    /// <param name="post">The post.</param>
    /// <returns>The boost.</returns>
    public async Task<Post?> Boost(
        User user,
        Post post)
    {
        if (!post.CanBeBoosted())
            return null;
        
        var boost = new Post()
        {
            Id = Ulid.NewUlid(),
            
            Author = user,
            AuthorId = user.Id,

            Boosting = post,
            BoostingId = post.Id,
            
            Visibility = post.Visibility
        };

        await repo.AddBoost(
            boost,
            post);

        if (!post.Author.IsRemote)
        {
            await notificationService.DispatchBoost(
                post.Author,
                user,
                post);
        }
        
        if (user.IsRemote)
            return boost;
        
        var announce = postRenderer.RenderBoostForNote(user, post);
        
        await federationService.SendToRelevantPostUsers(
            user,
            post,
            announce);

        return boost;
    }

    /// <summary>
    /// Handles a like.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="post"></param>
    public async Task Like(
        User actor,
        Post post)
    {
        if (await HasLiked(actor, post))
            return;
        
        var like = new PostLike()
        {
            Id = Ulid.NewUlid(),
            
            Post = post,
            PostId = post.Id,
            LikingUser = actor,
            LikingUserId = actor.Id
        };

        await repo.AddLike(
            like,
            post);
        
        if (!post.Author.IsRemote)
        {
            await notificationService.DispatchLike(
                post.Author,
                actor,
                post);
        }

        if (!post.Author.IsRemote || actor.IsRemote)
            return;

        var likeActivity = postRenderer.RenderLikeForNote(
            actor,
            post);
        
        await federationService.SendToRelevantPostUsers(
            actor,
            post,
            likeActivity);
    }

    /// <summary>
    /// Pins a post.
    /// </summary>
    /// <param name="post">The post.</param>
    public async Task Pin(
        Post post)
    {
        if (await repo.FindPinnedPost(post) is not null)
            return;
        
        var pinnedPost = new PinnedPost
        {
            Id = Ulid.NewUlid(),
            
            User = post.Author,
            UserId = post.AuthorId,

            Post = post,
            PostId = post.Id
        };

        await repo.AddPinnedPost(pinnedPost);

        if (post.Author.IsRemote)
            return;

        var add = postRenderer.RenderAddForPost(
            post.Author,
            post);
        
        await federationService.SendToFollowers(
            post.Author,
            add);
    }
    
    /// <summary>
    /// Unpins a post.
    /// </summary>
    /// <param name="post">The post.</param>
    public async Task Unpin(
        Post post)
    {
        var pinned = await repo.FindPinnedPost(post);
        if (pinned is null)
            return;
        
        await repo.DeletePinnedPost(pinned);

        if (post.Author.IsRemote)
            return;
        
        var remove = postRenderer.RenderRemoveForPost(
            post.Author,
            post);
        
        await federationService.SendToFollowers(
            post.Author,
            remove);
    }

    /// <summary>
    /// Undoes a like on a post done by an actor.
    /// </summary>
    /// <param name="actor">The actor which did the like.</param>
    /// <param name="post">The post.</param>
    public async Task UndoLike(
        User actor,
        Post post)
    {
        var like = await repo.FindLikeByIds(
            post.Id,
            actor.Id);

        if (like is null)
            return;

        await repo.RemoveLike(like);
        if (actor.IsRemote)
            return;

        var undoLike = postRenderer.RenderUndoLikeForNote(
            actor,
            post);
        
        await federationService.SendToRelevantPostUsers(
            actor,
            post,
            undoLike);
    }
    
    /// <summary>
    /// Undoes a boost on a post done by an actor.
    /// </summary>
    /// <param name="actor">The actor which did the like.</param>
    /// <param name="boost">The post.</param>
    public async Task UndoBoost(
        User actor,
        Post boost)
    {
        await repo.RemoveBoost(boost);
        if (actor.IsRemote)
            return;

        var undoBoost = postRenderer.RenderUndoAnnounceForNote(
            actor,
            boost.Boosting!);
        
        await federationService.SendToRelevantPostUsers(
            actor,
            boost.Boosting!,
            undoBoost);
    }

    /// <summary>
    /// Checks whether a user has already liked a post.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="post">The post.</param>
    /// <returns>Whether they have liked it.</returns>
    public async Task<bool> HasLiked(
        User user,
        Post post)
    {
        var like = await repo.FindLikeByIds(
            post.Id,
            user.Id);

        return like is not null;
    }
    
    /// <summary>
    /// Checks whether a user has already boosted a post.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="post">The post.</param>
    /// <returns>Whether they have boosted it.</returns>
    public async Task<bool> HasBoosted(
        User user,
        Post post)
    {
        var hasBoosted = await repo.CreateCustomQuery()
            .Where(p => p.AuthorId == user.Id)
            .Where(p => p.BoostingId == post.Id)
            .AnyAsync();

        return hasBoosted;
    }
    
    /// <summary>
    /// Checks a list for which posts the user has boosted the post.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="ids">The list of ids to look through.</param>
    /// <returns>The boosted posts.</returns>
    public async Task<IReadOnlyList<Ulid>> FindManyBoostedPosts(
        User user,
        IReadOnlyList<Ulid> ids)
    {
        var boostedPosts = await repo.CreateCustomQuery()
            .Where(p => p.AuthorId == user.Id)
            .Where(p => p.BoostingId != null && ids.Contains(p.BoostingId!.Value))
            .Select(p => p.BoostingId!.Value)
            .ToListAsync();

        return boostedPosts ?? [];
    }
    
    /// <summary>
    /// Checks a list for which posts the user has liked the post.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="ids">The list of ids to look through.</param>
    /// <returns>The liked posts.</returns>
    public async Task<IReadOnlyList<Ulid>> FindManyLikedPosts(
        User user,
        IReadOnlyList<Ulid> ids)
    {
        var likedPosts = await repo.CreateCustomLikeQuery()
            .Where(p => p.LikingUserId == user.Id)
            .Where(p => ids.Contains(p.PostId))
            .Select(p => p.PostId!)
            .ToListAsync();

        return likedPosts ?? [];
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
            if (mention.Href is null)
                return null;
            
            var user = await userManagementService.FetchFromRemoteId(mention.Href);
            if (user is null)
                continue;
            
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
    /// Collects the emojis for a note.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <param name="instance">The remote instance.</param>
    /// <returns>The emojis, if any exist.</returns>
    private async Task<IReadOnlyList<Emoji>?> CollectEmojis(
        ASNote note,
        RemoteInstance? instance)
    {
        if (note.Tags is null)
            return null;
        
        var asEmojis = note.Tags
            .OfType<ASEmoji>()
            .ToList();

        if (asEmojis.Count < 1)
            return null;

        return await emojiService.FetchFromActivityStreams(
            asEmojis,
            instance);
    }

    /// <summary>
    /// Collects the hashtags for a note.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <returns>The hashtags.</returns>
    private List<string>? CollectHashtags(
        ASNote note)
    {
        return note.Tags?.OfType<ASHashtag>()
            .Where(h => h.Name is not null)
            .Select(h => h.Name!)
            .ToList();
    }
    
    /// <summary>
    /// Imports an ActivityStreams note as a post.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <param name="author">The actor who was the author.</param>
    /// <param name="threadDepth">The current thread depth.</param>
    /// <returns>The post, if the import was successful.</returns>
    public async Task<Post?> ImportFromActivityStreams(
        ASNote note,
        User author,
        int threadDepth = 0)
    {
        var emojis = await CollectEmojis(
            note,
            author.ParentInstance);
        
        var post = new Post()
        {
            Id = Ulid.NewUlid(),
            RemoteId = note.Id,
            
            Context = Guid.NewGuid(),
            
            Author = author,
            AuthorId = author.Id,

            Content = htmlSanitizer.Sanitize(note.Content!),
            
            Sensitive = note.Sensitive ?? false,
            
            CreatedAt = note.PublishedAt?
                .ToUniversalTime() ?? DateTimeOffset.UtcNow,
            
            Visibility = note.GetPostVisibility(author),
            
            UserMentions = await CollectMentions(note),
            Tags = CollectHashtags(note),
            
            Emojis = emojis?
                .Select(e => e.Id.ToString())
                .ToList()
        };
        
        if (note.InReplyTo is not null)
        {
            // TODO: This should probably be offloaded to a background job...
            var parent = await FetchFromRemoteId(
                note.InReplyTo.Id,
                threadDepth + 1);
            
            if (parent is not null)
            {
                post.Parent = parent;
                post.Context = parent.Context;    
            }
        }
        
        // TODO: Resolve quote posts.
        await repo.Add(post);

        if (note.Attachments is not null &&
            note.Attachments.Count > 0)
        {
            await repo.ImportAttachmentsForNote(
                post,
                note.Attachments);
        }

        return post;
    }

    /// <summary>
    /// Fetches a post given its remote id.
    /// </summary>
    /// <param name="remoteId">The remote id of the post.</param>
    /// <param name="threadDepth">The current thread depth.</param>
    /// <returns>The post.</returns>
    public async Task<Post?> FetchFromRemoteId(
        string remoteId,
        int threadDepth = 0)
    {
        const int maxThreadRecursionDepth = 10;
        
        logger.LogInformation($"Fetching post {remoteId} with current depth of {threadDepth}.");
        
        var maybePost = await repo.FindByRemoteId(remoteId);
        if (maybePost is not null)
            return maybePost;

        if (threadDepth > maxThreadRecursionDepth)
            return null;

        var note = await resolver.Fetch<ASNote>(
            ASObject.Link(remoteId));

        if (note?.AttributedTo is null)
            return null;

        var author = await userManagementService.FetchFromRemoteId(
            note.AttributedTo.Id);
        
        if (author is null)
            return null;

        return await ImportFromActivityStreams(
            note,
            author,
            threadDepth);
    }
}