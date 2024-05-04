using Toki.ActivityPub.Models.Enums;

namespace Toki.Extensions;

/// <summary>
/// Extensions for the <see cref="NotificationType"/> enum.
/// </summary>
public static class NotificationTypeExtensions
{
    /// <summary>
    /// Converts a <see cref="NotificationType"/> to a Mastodon stringified type.
    /// </summary>
    /// <param name="notificationType">The notification type.</param>
    /// <returns>The Mastodon string representation.</returns>
    public static string ToMastodonNotificationType(this NotificationType notificationType) => notificationType switch
    {
        NotificationType.Mention => "mention",
        NotificationType.NewStatus => "status",
        NotificationType.Boost => "reblog",
        NotificationType.Follow => "follow",
        NotificationType.FollowRequest => "follow_request",
        NotificationType.Like => "favourite",
        NotificationType.Bite => "bite",
        _ => "" // TODO: Implement the rest of the Mastodon notification types.
    };
}