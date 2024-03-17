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
    /// Gets follow relations for both of these users.
    /// </summary>
    /// <param name="user1">The first user.</param>
    /// <param name="user2">The second user.</param>
    /// <returns>The follower relations.</returns>
    public async Task<IList<FollowerRelation>> GetFollowRelationsFor(
        User user1,
        User user2)
    {
        return await db.FollowerRelations
            .Where(fr => (fr.Follower == user1 && fr.Followee == user2) ||
                         (fr.Follower == user2 && fr.Followee == user1))
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets follow requests for both of these users.
    /// </summary>
    /// <param name="user1">The first user.</param>
    /// <param name="user2">The second user.</param>
    /// <returns>The follow requests.</returns>
    public async Task<IList<FollowRequest>> GetFollowRequestsFor(
        User user1,
        User user2)
    {
        return await db.FollowRequests
            .Where(fr => (fr.From == user1 && fr.To == user2) ||
                         (fr.From == user2 && fr.To == user1))
            .ToListAsync();
    }

    /// <summary>
    /// Adds a follow relation.
    /// </summary>
    /// <param name="fr">The follower relation.</param>
    public async Task AddFollow(FollowerRelation fr)
    {
        db.FollowerRelations.Add(fr);

        // Update the counts of both of the users.
        fr.Followee.FollowerCount++;
        fr.Follower.FollowingCount++;
        db.Users.Update(fr.Followee);
        db.Users.Update(fr.Follower);
        
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
    public async Task<FollowRequest?> FindFollowRequestById(Ulid id)
    {
        return await db.FollowRequests
            .Where(fr => fr.Id == id)
            .Include(fr => fr.From)
            .Include(fr => fr.To)
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
            Id = Ulid.NewUlid(),
            
            Follower = fr.From,
            FollowerId = fr.FromId,
            
            Followee = fr.To,
            FolloweeId = fr.ToId
        };

        await AddFollow(followRelation);
    }
}