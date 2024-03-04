using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Federation;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
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
    MessageFederationService federationService)
{
    /// <summary>
    /// Gets posts for a target user.
    /// </summary>
    /// <param name="target">The target user.</param>
    /// <param name="viewingAs">Who are we viewing this target's posts as? Blank if public.</param>
    /// <returns>An enumerable of posts.</returns>
    public async Task<IEnumerable<Post>> GetPostsForUser(
        User target,
        User? viewingAs = null)
    {
        var query = repo.CreateCustomQuery()
            .Where(post => post.AuthorId == target.Id)
            .Include(post => post.Parent)
            .Include(post => post.Author)
            .ThenInclude(author => author.FollowerRelations)
            .Include(post => post.Boosting)
            .ThenInclude(boost => boost!.Author)
            .Where(post => post.Visibility == PostVisibility.Public ||
                           post.Visibility == PostVisibility.Unlisted);
        
        return await query.ToListAsync();
    }
    
    /// <summary>
    /// Creates a new post.
    /// </summary>
    /// <param name="author">The author of the post.</param>
    /// <param name="content">The content of the post.</param>
    /// <param name="visibility">The visibility.</param>
    /// <returns>The created post.</returns>
    public async Task<Post?> Create(
        User author,
        string content,
        PostVisibility visibility)
    {
        if (author.IsRemote)
            return null;

        var post = new Post()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            
            Author = author,
            AuthorId = author.Id,

            Content = content,
            
            Visibility = visibility
        };

        await repo.Add(post);
        await federationService.SendToFollowers(
            author,
            postRenderer.RenderCreationForNote(post));
        
        return post;
    }

    /// <summary>
    /// Boosts a post.
    /// </summary>
    /// <param name="user">The user who's boosting.</param>
    /// <param name="post">The post.</param>
    public async Task Boost(
        User user,
        Post post)
    {
        if (!post.CanBeBoosted())
            return;
        
        var boost = new Post()
        {
            Id = Guid.NewGuid(),
            Author = user,
            AuthorId = user.Id,

            Boosting = post,
            BoostingId = post.Id,
            
            Visibility = post.Visibility
        };

        await repo.AddBoost(
            boost,
            post);
        
        if (!post.Author.IsRemote || user.IsRemote)
            return;
        
        var announce = postRenderer.RenderBoostForNote(user, post);
        
        // TODO: This is plainly wrong... but for testing I guess it's fine
        await federationService.SendToFollowers(
            user,
            announce);
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
        var like = new PostLike()
        {
            Id = Guid.NewGuid(),
            
            Post = post,
            PostId = post.Id,
            LikingUser = actor,
            LikingUserId = actor.Id
        };

        await repo.AddLike(
            like,
            post);

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
}