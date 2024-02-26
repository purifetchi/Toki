using System.Text.Json;
using Hangfire;
using Toki.ActivityPub.Jobs.Federation;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityStreams.Activities;

namespace Toki.ActivityPub.Federation;

/// <summary>
/// A helper class for message federation.
/// </summary>
/// <param name="followRepo">The user service.</param>
public class MessageFederationService(
    FollowRepository followRepo)
{
    /// <summary>
    /// Sends a message to the followers of an actor.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <param name="message">The message.</param>
    public async Task SendToFollowers(
        User actor,
        ASActivity message)
    {
        var data = JsonSerializer.Serialize(message);
        var followers = await followRepo.GetFollowersFor(actor);
        
        // TODO: Deduplicate inboxes (send to shared if applicable).
        
        BackgroundJob.Enqueue<MessageFederationJob>(job =>
            job.FederateMessage(data, followers.Select(f => f.Inbox)!, actor.Id, 0));
    }

    /// <summary>
    /// Sends a message from an actor directly to a target.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <param name="target">The target.</param>
    /// <param name="message">The message to send.</param>
    public async Task SendTo(
        User actor,
        User target,
        ASActivity message)
    {
        var data = JsonSerializer.Serialize(message);
        var targets = new List<string> { target.Inbox! };
        
        BackgroundJob.Enqueue<MessageFederationJob>(job =>
            job.FederateMessage(data, targets, actor.Id, 0));
    }
}