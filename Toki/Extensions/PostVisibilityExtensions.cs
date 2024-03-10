using Toki.ActivityPub.Models.Enums;

namespace Toki.Extensions;

/// <summary>
/// Extensions for the <see cref="PostVisibility"/> enum.
/// </summary>
public static class PostVisibilityExtensions
{
    /// <summary>
    /// Converts a <see cref="PostVisibility"/> to a Mastodon visibility.
    /// </summary>
    /// <param name="visibility">The visibility.</param>
    /// <returns>The resulting Mastodon visibility string.</returns>
    public static string ToMastodonString(this PostVisibility visibility) => visibility switch
    {
        PostVisibility.Public => "public",
        PostVisibility.Unlisted => "unlisted",
        PostVisibility.Followers => "private",
        _ => "direct"
    };
}