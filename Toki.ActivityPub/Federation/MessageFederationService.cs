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
    public async Task SendToFollowers<TActivity>(
        User actor,
        TActivity message)
    where TActivity: ASActivity
    {
        var data = JsonSerializer.Serialize(message);
        var followers = (await followRepo.GetFollowersFor(actor))
            .Where(a => a.IsRemote)
            .Select(f => f.Inbox!);
        
        // TODO: Deduplicate inboxes (send to shared if applicable).
        
        BackgroundJob.Enqueue<MessageFederationJob>(job =>
            job.FederateMessage(data, followers, actor.Id, 0));
    }

    /// <summary>
    /// Sends a message from an actor directly to a target.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <param name="target">The target.</param>
    /// <param name="message">The message to send.</param>
    public void SendTo<TActivity>(
        User actor,
        User target,
        TActivity message)
    where TActivity: ASActivity
    {
        var data = JsonSerializer.Serialize(message);
        var targets = new List<string> { target.Inbox! };
        
        BackgroundJob.Enqueue<MessageFederationJob>(job =>
            job.FederateMessage(data, targets, actor.Id, 0));
    }
}