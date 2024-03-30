using System.Text.Json;
using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Notifications;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.Users;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Jobs.Federation;

/// <summary>
/// An inbox handler job.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class InboxHandlerJob(
    ActivityPubResolver resolver,
    UserRelationService userRelationService,
    UserRepository repo,
    PostRepository postRepo,
    PostManagementService postManagementService,
    ILogger<InboxHandlerJob> logger,
    NotificationService notificationService)
{
    /// <summary>
    /// Handles an activity.
    /// </summary>
    /// <param name="objectJson">The activity json.</param>
    public async Task HandleActivity(string objectJson)
    {
        var activity = JsonSerializer.Deserialize<ASObject>(objectJson) as ASActivity;
        logger.LogInformation($"Received activity of type {activity?.Type} from {activity?.Actor.Id}");

        // Ensure we have the actual actor that is performing this task.
        var actor = await repo.FindByRemoteId(activity!.Actor.Id);
        if (actor is null)
        {
            var actorData = await resolver.Fetch<ASActor>(activity!.Actor);
            if (actorData is null)
                return;

            actor = await repo.ImportFromActivityStreams(actorData);
            if (actor is null)
            {
                logger.LogError($"Failed to retrieve actor {actorData.Id} for activity. Aborting.");
                return;
            }
        }
        
        // TODO: Handle every activity.
        await (activity switch
        {
            ASAccept accept => HandleAccept(accept),
            ASCreate create => HandleCreate(create, actor),
            ASFollow follow => userRelationService.HandleFromActivityStreams(follow),
            ASAnnounce announce => HandleAnnounce(announce, actor),
            ASLike like => HandleLike(like, actor),
            ASUndo undo => HandleUndo(undo, actor),
            ASUpdate update => HandleUpdate(update, actor),
            ASDelete delete => HandleDelete(delete, actor),
            ASReject reject => HandleReject(reject),
            
            _ => Task.Run(() =>
            {
                logger.LogWarning($"Dropped {activity.Id}, due to no handler present for {activity.Type}");
            })
        });
    }

    /// <summary>
    /// Handles the Update activity.
    /// </summary>
    /// <param name="update">The undo.</param>
    /// <param name="actor">The actor doing it.</param>
    private async Task HandleUpdate(ASUpdate update, User actor)
    {
        if (update.Object is null)
            return;
        
        var obj = await resolver.Fetch<ASObject>(update.Object);
        
        if (obj is ASActor actorData)
        {
            if (actorData.Id != actor.RemoteId)
                return;

            await repo.UpdateFromActivityStreams(actor, actorData);
            return;
        }
        
        logger.LogWarning($"Dropped Update {update.Id}, due to no handler present for {obj.Type}");
    }

    /// <summary>
    /// Handles the Undo activity.
    /// </summary>
    /// <param name="undo">The undo.</param>
    /// <param name="actor">The actor doing it.</param>
    private async Task HandleUndo(ASUndo undo, User actor)
    {
        if (undo.Object is null)
            return;
        
        var asObject = await resolver.Fetch<ASObject>(undo.Object);
        if (asObject is not ASActivity activity)
        {
            logger.LogWarning($"Someone just attempted to undo an object that's not an activity? Id={asObject?.Id}");
            return;
        }

        if (activity.Actor.Id != actor.RemoteId)
        {
            logger.LogWarning($"{actor.RemoteId} attempted to undo an activity performed by {activity.Actor.Id}.");
            return;
        }
        
        // TODO: Undo the activity  here.
    }
    
    /// <summary>
    /// Handles the Delete activity.
    /// </summary>
    /// <param name="delete">The delete.</param>
    /// <param name="actor">The actor doing it.</param>
    private async Task HandleDelete(ASDelete delete, User actor)
    {
        if (delete.Object is null)
            return;
        
        // We won't be fetching the object here, for it might just not exist.
        // Let's try to walk and see what is there for us to delete...

        var post = await postRepo.FindByRemoteId(delete.Object.Id);
        if (post is not null)
        {
            if (post.Author != actor)
            {
                logger.LogWarning($"{actor.RemoteId} attempted to delete a post made by {post.Author.Id}.");
                return;
            }
            
            await postManagementService.Delete(post);
            return;
        }
        
        // TODO: Handle ASDelete for Actors too.
    }

    /// <summary>
    /// Handles the Create activity.
    /// </summary>
    private async Task HandleCreate(ASCreate create, User actor)
    {
        if (create.Object is null)
            return;

        var obj = await resolver.Fetch<ASObject>(create.Object);
        if (obj is ASNote note)
        {
            var post = await postRepo.ImportFromActivityStreams(note, actor);
            if (post is null)
                return;
            
            await notificationService.DispatchAllNotificationsForPost(post);
        }
    }
    
    /// <summary>
    /// Handles the Accept activity.
    /// </summary>
    private async Task HandleAccept(ASAccept accept)
    {
        if (accept.Object is null)
            return;

        if (await userRelationService.TryHandleRemoteFollowAccept(accept))
            return;
        
        logger.LogWarning($"Accept for unknown object {accept.Object.Id}.");
    }
    
    /// <summary>
    /// Handles the Reject activity.
    /// </summary>
    private async Task HandleReject(ASReject reject)
    {
        if (reject.Object is null)
            return;

        if (await userRelationService.TryHandleRemoteFollowReject(reject))
            return;
        
        logger.LogWarning($"Reject for unknown object {reject.Object.Id}.");
    }

    /// <summary>
    /// Handles the Announce activity.
    /// </summary>
    private async Task HandleAnnounce(ASAnnounce announce, User actor)
    {
        if (announce.Object is null)
            return;
        
        // TODO: Lemmy does announces for any activity related to posts in a group...
        //       We're currently only expecting it to mean boosts.
        var post = await postRepo.FindByRemoteId(announce.Object.Id);
        if (post is null)
        {
            logger.LogWarning($"Received an announce for an either non-existent post, or a completely different object: {announce.Object.Id}");
            return;
        }

        await postManagementService.Boost(
            actor,
            post);
    }

    /// <summary>
    /// Handles the Like activity.
    /// </summary>
    private async Task HandleLike(ASLike like, User actor)
    {
        if (like.Object is null)
            return;

        var post = await postRepo.FindByRemoteId(like.Object.Id);
        if (post is null)
        {
            // TODO: Fetch the post here.
            logger.LogWarning($"Received a like for a non-existent post. {like.Object.Id}");
            return;
        }

        await postManagementService.Like(
            actor,
            post);
    }
}