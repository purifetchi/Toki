using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;

namespace Toki.ActivityPub.Notifications;

/// <summary>
/// The service responsible for dispatching notifications.
/// </summary>
public class NotificationService(
    NotificationRepository repo,
    UserRepository userRepository)
{
    /// <summary>
    /// Attempts to add the notification, if it makes sense to do so.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <returns>The added notification on success, nothing else.</returns>
    private async Task<Notification?> MaybeAddNotification(
        Notification notification)
    {
        // We aren't interested in what we've done to ourselves.
        if (notification.ActorId == notification.TargetId)
            return null;
        
        await repo.Add(notification);
        return notification;
    }
    
    /// <summary>
    /// Dispatches a like notification.
    /// </summary>
    /// <param name="author">The author of the post.</param>
    /// <param name="causer">The person liking the post.</param>
    /// <param name="note">The post that was liked.</param>
    /// <returns>The dispatched notification.</returns>
    public async Task<Notification?> DispatchLike(
        User author,
        User causer,
        Post note)
    {
        var notif = new Notification
        {
            Id = Ulid.NewUlid(),
            Type = NotificationType.Like,
            
            Target = author,
            TargetId = author.Id,

            Actor = causer,
            ActorId = causer.Id,

            RelevantPost = note,
            RelevantPostId = note.Id
        };

        return await MaybeAddNotification(notif);
    }
    
    /// <summary>
    /// Dispatches a boost notification.
    /// </summary>
    /// <param name="author">The author of the post.</param>
    /// <param name="causer">The person boosting the post.</param>
    /// <param name="note">The post that was boosted.</param>
    /// <returns>The dispatched notification.</returns>
    public async Task<Notification?> DispatchBoost(
        User author,
        User causer,
        Post note)
    {
        var notif = new Notification
        {
            Id = Ulid.NewUlid(),
            Type = NotificationType.Boost,
            
            Target = author,
            TargetId = author.Id,

            Actor = causer,
            ActorId = causer.Id,

            RelevantPost = note,
            RelevantPostId = note.Id
        };

        return await MaybeAddNotification(notif);
    }
    
    /// <summary>
    /// Dispatches a mention notification.
    /// </summary>
    /// <param name="mentioned">The mentioned person.</param>
    /// <param name="mentioner">The person mentioning them.</param>
    /// <param name="note">The post that was part of the mention.</param>
    /// <returns>The dispatched notification.</returns>
    public async Task<Notification?> DispatchMention(
        User mentioned,
        User mentioner,
        Post note)
    {
        var notif = new Notification
        {
            Id = Ulid.NewUlid(),
            Type = NotificationType.Mention,
            
            Target = mentioned,
            TargetId = mentioned.Id,

            Actor = mentioner,
            ActorId = mentioner.Id,

            RelevantPost = note,
            RelevantPostId = note.Id
        };

        return await MaybeAddNotification(notif);
    }

    /// <summary>
    /// Dispatches a follow notification.
    /// </summary>
    /// <param name="target">The target user.</param>
    /// <param name="follower">The follower.</param>
    /// <returns>The dispatched notification.</returns>
    public async Task<Notification?> DispatchFollow(
        User target,
        User follower)
    {
        var notif = new Notification
        {
            Id = Ulid.NewUlid(),
            Type = NotificationType.Follow,
            
            Target = target,
            TargetId = target.Id,

            Actor = follower,
            ActorId = follower.Id
        };
        
        return await MaybeAddNotification(notif);
    }
    
    /// <summary>
    /// Dispatches all of the notifications for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The notifications.</returns>
    public async Task<IReadOnlyList<Notification?>?> DispatchAllNotificationsForPost(
        Post post)
    {
        var notifs = new List<Notification?>();
        
        // Send out mentions
        if (post.UserMentions is not null)
        {
            foreach (var mention in post.UserMentions.Where(mention => !mention.IsRemoteMention()))
            {
                var user = await userRepository.FindById(
                    Ulid.Parse(mention.Id));

                if (user is null || user == post.Author)
                    continue;

                var notif = await DispatchMention(
                    user,
                    post.Author,
                    post);

                if (notif is null)
                    continue;
                
                notifs.Add(notif);
            }
        }
        
        return notifs;
    }

    /// <summary>
    /// Dispatches a follow request notification.
    /// </summary>
    /// <param name="target">The target user.</param>
    /// <param name="follower">The follower.</param>
    /// <returns>The dispatched notification.</returns>
    public async Task<Notification?> DispatchFollowRequest(
        User target,
        User follower)
    {
        var notif = new Notification
        {
            Id = Ulid.NewUlid(),
            Type = NotificationType.FollowRequest,
            
            Target = target,
            TargetId = target.Id,

            Actor = follower,
            ActorId = follower.Id
        };
        
        return await MaybeAddNotification(notif);
    }
}