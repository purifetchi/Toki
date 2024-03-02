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
    /// Gets the follower's inboxes (either direct or shared) for a given user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The inboxes.</returns>
    public async Task<IEnumerable<string>> GetFollowerInboxesFor(User user)
    {
        return await db.FollowerRelations
            .Include(fr => fr.Follower)
            .Include(fr => fr.Follower.ParentInstance)
            .Where(fr => fr.FolloweeId == user.Id)
            .Select(fr => fr.Follower)
            .Where(f => f.IsRemote)
            .Select(f => f.ParentInstance != null ? f.ParentInstance.SharedInbox! : f.Inbox! )
            .Distinct()
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
    /// Finds a follow request by an id.
    /// </summary>
    /// <param name="id">The id of the request.</param>
    /// <returns>The follow request.</returns>
    public async Task<FollowRequest?> FindFollowRequestById(Guid id)
    {
        return await db.FollowRequests
            .Where(fr => fr.Id == id)
            .FirstOrDefaultAsync();
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