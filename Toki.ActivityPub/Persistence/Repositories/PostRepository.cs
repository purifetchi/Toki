using Microsoft.EntityFrameworkCore;
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
    UserRepository userRepo)
{
    /// <summary>
    /// Finds a post by its remote id.
    /// </summary>
    /// <param name="remoteId">The remote id of the post.</param>
    /// <returns>The post if it exists.</returns>
    public async Task<Post?> FindByRemoteId(string remoteId)
    {
        return await db.Posts
            .FirstOrDefaultAsync(post => post.RemoteId == remoteId);
    }
    
    /// <summary>
    /// Imports an ActivityStreams note as a post.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <param name="actor">The actor who was the author.</param>
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

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        return post;
    }
}