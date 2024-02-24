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
    /// Imports an ActivityStreams note as a post.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <param name="actor">The actor who was the author.</param>
    /// <returns>The post, if the import was successful.</returns>
    public async Task<Post?> ImportFromActivityStreams(
        ASNote note,
        ASActor actor)
    {
        var author = await userRepo.FindByRemoteId(actor.Id) 
                     ?? await userRepo.ImportFromActivityStreams(actor);

        if (author is null)
            return null;

        var post = new Post()
        {
            Id = Guid.NewGuid(),
            Author = author,
            AuthorId = author.Id,

            Content = note.Content!
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        return post;
    }
}