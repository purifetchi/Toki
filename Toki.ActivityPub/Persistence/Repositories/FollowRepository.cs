using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// The repository for follows and follow requests.
/// </summary>
/// <param name="db">The database.</param>
public class FollowRepository(
    TokiDatabaseContext db)
{
    /// <summary>
    /// Gets followers for a given user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The users that follow them.</returns>
    public async Task<IEnumerable<User>> GetFollowersFor(User user)
    {
        return await db.FollowerRelations
            .Where(fr => fr.Followee == user)
            .Select(fr => fr.Follower)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the followed users for a given user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The users that they follow.</returns>
    public async Task<IEnumerable<User>> GetFollowingFor(User user)
    {
        return await db.FollowerRelations
            .Where(fr => fr.Follower == user)
            .Select(fr => fr.Followee)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a follow relation.
    /// </summary>
    /// <param name="fr">The follower relation.</param>
    public async Task AddFollow(FollowerRelation fr)
    {
        db.FollowerRelations.Add(fr);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Adds a follow request.
    /// </summary>
    /// <param name="fr">The follower request.</param>
    public async Task AddFollowRequest(FollowRequest fr)
    {
        db.FollowRequests.Add(fr);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Transforms a follow request into a follow.
    /// </summary>
    /// <param name="fr">The follow request.</param>
    public async Task TransformIntoFollow(FollowRequest fr)
    {
        db.FollowRequests.Remove(fr);

        var followRelation = new FollowerRelation()
        {
            Id = Guid.NewGuid(),
            
            Follower = fr.From,
            FollowerId = fr.FromId,
            
            Followee = fr.To,
            FolloweeId = fr.ToId
        };

        await AddFollow(followRelation);
    }
}