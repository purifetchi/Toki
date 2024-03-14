using Toki.ActivityPub.Models.Enums;

namespace Toki.ActivityPub.Models;

/// <summary>
/// A notification.
/// </summary>
public class Notification : AbstractModel
{
    /// <summary>
    /// Creates a new notification.
    /// </summary>
    public Notification()
    {
        CreatedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// The type of this notification.
    /// </summary>
    public required NotificationType Type { get; init; }

    /// <summary>
    /// The target of this notification.
    /// </summary>
    public required User Target { get; init; }
    
    /// <summary>
    /// The id of the target user.
    /// </summary>
    public required Ulid TargetId { get; init; }
    
    /// <summary>
    /// The actor of the notification.
    /// </summary>
    public required User Actor { get; init; }
    
    /// <summary>
    /// The id of the actor user.
    /// </summary>
    public required Ulid ActorId { get; init; }
    
    /// <summary>
    /// When was this notification created?
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>
    /// The post relevant to the notification.
    /// </summary>
    public Post? RelevantPost { get; set; }
    
    /// <summary>
    /// The id of the relevant post.
    /// </summary>
    public Ulid? RelevantPostId { get; set; }
}