using Toki.ActivityPub.Models.Posts;

namespace Toki.ActivityPub.Extensions;

/// <summary>
/// Extensions for the <see cref="PostMention"/> model.
/// </summary>
public static class PostMentionExtensions
{
    /// <summary>
    /// Checks if this mention is for a remote user. (That is, it contains an at symbol.)
    /// </summary>
    /// <param name="mention">The post mention.</param>
    /// <returns>Whether it is for a remote user.</returns>
    public static bool IsRemoteMention(this PostMention mention) =>
        mention.Handle.Contains('@');
}