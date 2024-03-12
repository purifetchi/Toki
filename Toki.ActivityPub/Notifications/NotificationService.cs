using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;

namespace Toki.ActivityPub.Notifications;

/// <summary>
/// The service responsible for dispatching notifications.
/// </summary>
public class NotificationService(
    NotificationRepository repo)
{
    /// <summary>
    /// Dispatches a like notification.
    /// </summary>
    /// <param name="author">The author of the post.</param>
    /// <param name="causer">The person liking the post.</param>
    /// <param name="note">The post that was liked.</param>
    /// <returns>The dispatched notification.</returns>
    public async Task<Notification> DispatchLike(
        User author,
        User causer,
        Post note)
    {
        var notif = new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Like,
            
            Target = author,
            TargetId = author.Id,

            Actor = causer,
            ActorId = causer.Id,

            RelevantPost = note,
            RelevantPostId = note.Id
        };

        await repo.Add(notif);
        return notif;
    }
    
    /// <summary>
    /// Dispatches a boost notification.
    /// </summary>
    /// <param name="author">The author of the post.</param>
    /// <param name="causer">The person boosting the post.</param>
    /// <param name="note">The post that was boosted.</param>
    /// <returns>The dispatched notification.</returns>
    public async Task<Notification> DispatchBoost(
        User author,
        User causer,
        Post note)
    {
        var notif = new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Boost,
            
            Target = author,
            TargetId = author.Id,

            Actor = causer,
            ActorId = causer.Id,

            RelevantPost = note,
            RelevantPostId = note.Id
        };

        await repo.Add(notif);
        return notif;
    }

    /// <summary>
    /// Dispatches a follow notification.
    /// </summary>
    /// <param name="target">The target user.</param>
    /// <param name="follower">The follower.</param>
    /// <returns>The dispatched notification.</returns>
    public async Task<Notification> DispatchFollow(
        User target,
        User follower)
    {
        var notif = new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Follow,
            
            Target = target,
            TargetId = target.Id,

            Actor = follower,
            ActorId = follower.Id
        };
        
        await repo.Add(notif);
        return notif;
    }
}