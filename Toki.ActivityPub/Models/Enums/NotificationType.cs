namespace Toki.ActivityPub.Models.Enums;

/// <summary>
/// Type of the notification.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Someone mentioned you in their status
    /// </summary>
    Mention,
    
    /// <summary>
    /// Someone you enabled notifications for has posted a status
    /// </summary>
    NewStatus,
    
    /// <summary>
    /// Someone boosted one of your statuses
    /// </summary>
    Boost,
    
    /// <summary>
    /// Someone followed you
    /// </summary>
    Follow,
    
    /// <summary>
    /// Someone requested to follow you
    /// </summary>
    FollowRequest,
    
    /// <summary>
    /// Someone liked one of your statuses
    /// </summary>
    Like,
}