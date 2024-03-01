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
        // TODO: Persist this somehow in the DB.
        var announce = postRenderer.RenderBoostForNote(user, post);
        
        // TODO: This is plainly wrong... but for testing I guess it's fine
        await federationService.SendToFollowers(
            user,
            announce);
    }
}