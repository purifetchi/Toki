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
    /// Checks if a follower relation exists between two users.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>Whether the follower relation exists.</returns>
    public async Task<bool> FollowRelationExistsBetween(
        User source,
        User target)
    {
        return await db.FollowerRelations
            .AnyAsync(fr => fr.Followee == target && fr.Follower == source);
    }
    
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
    /// Gets the pending follow requests for a given user, in reverse chronological order.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Their pending follow requests.</returns>
    public async Task<IEnumerable<User>> GetFollowRequestsFor(User user)
    {
        return await db.FollowRequests
            .Where(fr => fr.To == user)
            .Include(fr => fr.From)
            .OrderByDescending(fr => fr.Id)
            .Select(fr => fr.From)
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
            .Select(f => f.ParentInstance != null && f.ParentInstance.SharedInbox != null ? f.ParentInstance.SharedInbox! : f.Inbox! )
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
    public async Task<IList<FollowRequest>> GetFollowRequestRelationsFor(
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
    /// Removes a follow relation.
    /// </summary>
    /// <param name="fr">The follower relation.</param>
    public async Task RemoveFollow(FollowerRelation fr)
    {
        fr.Followee.FollowerCount--;
        fr.Follower.FollowingCount--;
        db.Users.Update(fr.Followee);
        db.Users.Update(fr.Follower);
        
        db.FollowerRelations.Remove(fr);
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
    /// Removes a follow request.
    /// </summary>
    /// <param name="fr">The follower request.</param>
    public async Task RemoveFollowRequest(FollowRequest fr)
    {
        db.FollowRequests.Remove(fr);
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
    /// Finds a follow request by a remote id.
    /// </summary>
    /// <param name="remoteId">The remote id of the request.</param>
    /// <returns>The follow request.</returns>
    public async Task<FollowRequest?> FindFollowRequestByRemoteId(string remoteId)
    {
        return await db.FollowRequests
            .Where(fr => fr.RemoteId == remoteId)
            .Include(fr => fr.From)
            .Include(fr => fr.To)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Finds a follower relation by a remote id.
    /// </summary>
    /// <param name="remoteId">The remote id of the request.</param>
    /// <returns>The follower relation.</returns>
    public async Task<FollowerRelation?> FindFollowByRemoteId(string remoteId)
    {
        return await db.FollowerRelations
            .Where(fr => fr.RemoteId == remoteId)
            .Include(fr => fr.Followee)
            .Include(fr => fr.Follower)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Finds a follow request originating from some user and directed at another user.
    /// </summary>
    /// <param name="to">The user the request is directed at.</param>
    /// <param name="from">The user the request is originating from.</param>
    /// <returns>A <see cref="FollowRequest"/> if one exists.</returns>
    public async Task<FollowRequest?> FindFollowRequestByToAndFrom(
        User from,
        User to)
    {
        return await db.FollowRequests
            .FirstOrDefaultAsync(fr => fr.ToId == to.Id && fr.FromId == from.Id);
    }
    
    /// <summary>
    /// Finds a follower relation originating from some user and directed at another user.
    /// </summary>
    /// <param name="to">The user the request is directed at.</param>
    /// <param name="from">The user the request is originating from.</param>
    /// <returns>A <see cref="FollowerRelation"/> if one exists.</returns>
    public async Task<FollowerRelation?> FindRelationByFromAndTo(
        User from,
        User to)
    {
        return await db.FollowerRelations
            .Include(fr => fr.Follower)
            .Include(fr => fr.Followee)
            .FirstOrDefaultAsync(fr => fr.FolloweeId == to.Id && fr.FollowerId == from.Id);
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
            
            RemoteId = fr.RemoteId,
            
            Follower = fr.From,
            FollowerId = fr.FromId,
            
            Followee = fr.To,
            FolloweeId = fr.ToId
        };

        await AddFollow(followRelation);
    }
}