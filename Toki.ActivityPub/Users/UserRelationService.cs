using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Federation;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Users;

/// <summary>
/// Manages the relations between users.
/// </summary>
public class UserRelationService(
    FollowRepository followRepo,
    UserRepository repo,
    InstancePathRenderer pathRenderer,
    MessageFederationService messageFederation,
    ILogger<UserRelationService> logger)
{
    /// <summary>
    /// Requests a follow from one user to another.
    /// </summary>
    /// <param name="from">The source user.</param>
    /// <param name="to">The target user.</param>
    public async Task RequestFollow(
        User from,
        User to)
    {
        var request = await CreateFollowRequest(
            from,
            to);

        var activity = new ASFollow()
        {
            Id = request.RemoteId!,
            Actor = ASObject.Link(pathRenderer.GetPathToActor(from)),
            Object = ASObject.Link(to.RemoteId!)
        };
        
        await messageFederation.SendTo(
            to,
            from,
            activity);
    }

    /// <summary>
    /// Tries to handle a remote <see cref="ASAccept"/> for a follow.
    /// </summary>
    /// <param name="accept">The accept.</param>
    /// <returns>Whether it was handled or not.</returns>
    public async Task<bool> TryHandleRemoteFollowAccept(
        ASAccept accept)
    {
        var id = accept.Object!
            .Id
            .Split('/')
            .Last();

        if (!Guid.TryParse(id, out var guid))
            return false;

        var fr = await followRepo.FindFollowRequestById(guid);
        if (fr is null)
            return false;

        await followRepo.TransformIntoFollow(fr);
        return true;
    }

    /// <summary>
    /// Handles an ActivityStreams follow.
    /// </summary>
    /// <param name="follow">The follow.</param>
    public async Task HandleFromActivityStreams(ASFollow follow)
    {
        // Get the local user
        var subject = await repo.FindByRemoteId(follow.Object!.Id);
        if (subject is null || subject.IsRemote)
            return;

        var actor = await repo.FindByRemoteId(follow.Actor.Id);
        if (actor is null)
            return;

        if (subject.RequiresFollowApproval)
        {
            await CreateFollowRequest(actor, subject, follow.Id);
        }
        else
        {
            await AcceptFollowRequest(new FollowRequest
            {
                Id = Guid.NewGuid(),
                
                From = actor,
                FromId = actor.Id,
                To = subject,
                ToId = subject.Id,
                RemoteId = follow.Id
            });
        }
    }

    /// <summary>
    /// Creates a follow from one user to another.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    private async Task<FollowerRelation> CreateFollow(
        User from,
        User to)
    {
        logger.LogDebug($"Created a follow relation from {from.DisplayName} to {to.DisplayName}");
        
        var fr = new FollowerRelation()
        {
            Id = Guid.NewGuid(),

            Follower = from,
            FollowerId = from.Id,

            Followee = to,
            FolloweeId = to.Id
        };

        await followRepo.AddFollow(fr);
        return fr;
    }

    /// <summary>
    /// Accepts a follow request.
    /// </summary>
    /// <param name="fr">The follow request.</param>
    private async Task AcceptFollowRequest(
        FollowRequest fr)
    {
        var actorPath = pathRenderer.GetPathToActor(fr.To);
        
        var relation = await CreateFollow(fr.From, fr.To);
        var accept = new ASAccept
        {
            Id = $"{actorPath}#accepts/follows/{fr.Id}",
            Actor = ASObject.Link(actorPath),
            Object = ASObject.Link(fr.RemoteId!)
        };
            
        await messageFederation.SendTo(
            fr.To,
            fr.From,
            accept);
    }
    
    /// <summary>
    /// Creates a follow request from one user to another.
    /// </summary>
    /// <param name="from">The source user.</param>
    /// <param name="to">The target user.</param>
    /// <param name="remoteId">The remote id, if the follow request originated from elsewhere.</param>
    /// <returns>The created follow request.</returns>
    private async Task<FollowRequest> CreateFollowRequest(
        User from,
        User to,
        string? remoteId = null)
    {
        logger.LogDebug($"Created a follow request from {from.DisplayName} to {to.DisplayName}");
        
        var id = Guid.NewGuid();
        var fr = new FollowRequest
        {
            Id = id,
            CreatedAt = DateTime.Now,
            RemoteId = remoteId ?? 
                       $"{pathRenderer.GetPathToActor(from)}#follows/{id}",

            From = from,
            FromId = from.Id,

            To = to,
            ToId = to.Id
        };

        await followRepo.AddFollowRequest(fr);
        return fr;
    }
}