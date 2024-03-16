using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Federation;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.DTO;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Notifications;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;

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
    NotificationService notificationService)
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

        var post = new Post()
        {
            Id = Ulid.NewUlid(),
            Context = creationRequest.InReplyTo?.Context ??
                      Guid.NewGuid(),
            
            Author = creationRequest.Author,
            AuthorId = creationRequest.Author.Id,

            Content = creationRequest.Content,
            
            Sensitive = creationRequest.IsSensitive,
            ContentWarning = creationRequest.ContentWarning,
            
            Visibility = creationRequest.Visibility,
            
            Parent = creationRequest.InReplyTo,
            ParentId = creationRequest.InReplyTo?.Id,
            
            Attachments = creationRequest.Media
        };

        await repo.Add(post);
        await federationService.SendToFollowers(
            creationRequest.Author,
            postRenderer.RenderCreationForNote(post));
        
        return post;
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
        
        // TODO: This is plainly wrong... but for testing I guess it's fine
        await federationService.SendToFollowers(
            user,
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
        
        // TODO: This is plainly wrong... but for testing I guess it's fine
        await federationService.SendToFollowers(
            actor,
            likeActivity);
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
}