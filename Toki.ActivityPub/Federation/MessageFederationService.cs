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
/// <param name="postRepo">The post repository.</param>
public class MessageFederationService(
    FollowRepository followRepo,
    PostRepository postRepo)
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
        var followers = await followRepo.GetFollowerInboxesFor(actor);
        
        BackgroundJob.Enqueue<MessageFederationJob>(job =>
            job.FederateMessage(data, followers, actor.Id, 0));
    }

    /// <summary>
    /// Sends an update to all of the relevant users for a post (mentioned, author and followers).
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <param name="post">The post.</param>
    /// <param name="message">The message.</param>
    public async Task SendToRelevantPostUsers<TActivity>(
        User actor,
        Post post,
        TActivity message)
        where TActivity : ASActivity
    {
        var data = JsonSerializer.Serialize(message);
        var inboxes = (await followRepo.GetFollowerInboxesFor(actor))
            .Concat(await postRepo.GetInboxesForRelevantPostUsers(post))
            .Distinct();
        
        BackgroundJob.Enqueue<MessageFederationJob>(job =>
            job.FederateMessage(data, inboxes, actor.Id, 0));
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