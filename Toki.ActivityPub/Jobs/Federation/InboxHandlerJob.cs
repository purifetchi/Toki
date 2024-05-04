using System.Text.Json;
using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Bites;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Notifications;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.Users;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Activities.Extensions;
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
    FollowRepository followRepo,
    PostManagementService postManagementService,
    UserManagementService userManagementService,
    ILogger<InboxHandlerJob> logger,
    NotificationService notificationService,
    BiteService biteService)
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
        var actor = await userManagementService.FetchFromRemoteId(activity!.Actor.Id);
        if (actor is null)
        {
            logger.LogError($"Failed to retrieve actor {activity!.Actor.Id} for activity. Aborting.");
            return;
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
            ASAdd add => HandleAdd(add, actor),
            ASRemove remove => HandleRemove(remove, actor),
            ASBite bite => HandleBite(bite, actor),
            
            _ => Task.Run(() =>
            {
                logger.LogWarning($"Dropped {activity.Id}, due to no handler present for {activity.Type}");
            })
        });
    }

    /// <summary>
    /// Handles the Add activity.
    /// </summary>
    /// <param name="add">The add.</param>
    /// <param name="actor">The actor doing it.</param>
    private async Task HandleAdd(ASAdd add, User actor)
    {
        if (add.Object is null)
            return;
        
        var obj = await resolver.Fetch<ASObject>(add.Object);
        
        // NOTE: ActivityPub specifies that the sending server needs to specify a 'target' for the Add and Remove
        //       activities... but Pleroma just sends them without it. The target then just implicitly becomes
        //       the user's featured collection. Is there any other software that actually uses Add/Remove outside
        //       of pinned posts?
        if (obj is ASNote note)
        {
            if (note.AttributedTo?.Id != actor.RemoteId)
                return;

            var post = await postManagementService.FetchFromRemoteId(note.Id);
            if (post is null)
                return;
            
            await postManagementService.Pin(post);
        }
    }
    
    /// <summary>
    /// Handles the Remove activity.
    /// </summary>
    /// <param name="remove">The remove.</param>
    /// <param name="actor">The actor doing it.</param>
    private async Task HandleRemove(ASRemove remove, User actor)
    {
        if (remove.Object is null)
            return;
        
        var obj = await resolver.Fetch<ASObject>(remove.Object);
        
        // NOTE: See note above.
        if (obj is ASNote note)
        {
            if (note.AttributedTo?.Id != actor.RemoteId)
                return;

            var post = await postManagementService.FetchFromRemoteId(note.Id);
            if (post is null)
                return;
            
            await postManagementService.Unpin(post);
        }
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

        if (string.IsNullOrEmpty(update.Object.Id))
        {
            logger.LogWarning($"Received an Update for an object that doesn't point to anything.");
            return;
        }
        
        var obj = await resolver.Fetch<ASObject>(update.Object);
        
        if (obj is ASActor actorData)
        {
            if (actorData.Id != actor.RemoteId)
                return;

            await repo.UpdateFromActivityStreams(actor, actorData);
            return;
        }
        
        if (obj is ASNote noteData)
        {
            if (noteData.AttributedTo is not null &&
                noteData.AttributedTo.Id != actor.RemoteId)
            {
                return;
            }
            
            var originalPost = await postRepo.FindByRemoteId(
                noteData.Id);

            // If the original post is null, we can just import the updated data as a new post.
            if (originalPost is null)
            {
                await postManagementService.ImportFromActivityStreams(
                    noteData,
                    actor);
                
                return;
            }
            
            if (originalPost.Author.RemoteId != actor.RemoteId)
                return;

            await postManagementService.UpdateFromActivityStreams(
                originalPost,
                actor,
                noteData);

            return;
        }
        
        logger.LogWarning($"Dropped Update {update.Id}, due to no handler present for {update.Object?.Type}");
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
        
        logger.LogInformation($"Undoing {undo.Object.Id}");
        
        // Check if this is a boost.
        var maybeBoost = await postRepo.FindByRemoteId(undo.Object.Id);
        if (maybeBoost?.BoostingId != null)
        {
            if (maybeBoost.AuthorId != actor.Id)
            {
                logger.LogWarning($"{actor.RemoteId} attempted to undo a boost performed by {maybeBoost.AuthorId}.");
                return;
            }

            await postManagementService.UndoBoost(
                actor,
                maybeBoost);

            return;
        }

        // Check if this is a follow.
        var maybeFollow = await followRepo.FindFollowByRemoteId(undo.Object.Id);
        if (maybeFollow is not null)
        {
            if (maybeFollow.FollowerId != actor.Id)
            {
                logger.LogWarning($"{actor.RemoteId} attempted to undo a follow performed by {maybeFollow.Follower.Handle}.");
                return;
            }

            await userRelationService.Unfollow(
                maybeFollow.Follower,
                maybeFollow.Followee);

            return;
        }

        // TODO: We need to make likes also remoteable.
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
            var post = await postManagementService.ImportFromActivityStreams(note, actor);
            if (post is null)
                return;
            
            await notificationService.DispatchAllNotificationsForPost(post);
        }
        else if (obj is ASVideo video)
        {
            var post = await postManagementService.ImportFromActivityStreams(
                video.MockASNote(), 
                actor);
            
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
        var post = await postManagementService.FetchFromRemoteId(announce.Object.Id);
        if (post is null)
        {
            logger.LogWarning($"Received an announce for an either non-existent post, or a completely different object: {announce.Object.Id}");
            return;
        }

        await postManagementService.Boost(
            actor,
            post,
            announce.Id);
    }

    /// <summary>
    /// Handles the Like activity.
    /// </summary>
    private async Task HandleLike(ASLike like, User actor)
    {
        if (like.Object is null)
            return;

        var post = await postManagementService.FetchFromRemoteId(like.Object.Id);
        if (post is null)
        {
            logger.LogWarning($"Received a like for a non-existent post. {like.Object.Id}");
            return;
        }

        await postManagementService.Like(
            actor,
            post);
    }

    /// <summary>
    /// Handles the Bite activity.
    /// </summary>
    private async Task HandleBite(ASBite bite, User actor)
    {
        if (bite.Target is null)
            return;
        
        // Check if the object is biteable (is a user for now).
        var maybeUser = await repo.FindByRemoteId(bite.Target.Id);
        if (maybeUser is not null)
        {
            await biteService.Bite(
                actor,
                maybeUser);

            return;
        }
        
        // When receiving a Bite targetting an object whose type isn't explicitly implemented to be bitten,
        // implementations should fall back to biting the Actor in the to field
        if (bite.To?.Count > 0)
        {
            var maybeToUser = await repo.FindByRemoteId(bite.To[0]);
            if (maybeToUser is not null)
            {
                await biteService.Bite(
                    actor,
                    maybeToUser);

                return;
            }
        }
        
        // TODO: then the actor field in the target object, and then the attributedTo field in the target object.
    }
}