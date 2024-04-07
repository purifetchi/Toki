using Toki.ActivityPub.Models;

namespace Toki.ActivityPub.Renderers;

/// <summary>
/// A renderer for microformats links.
/// </summary>
public class MicroformatsRenderer(
    InstancePathRenderer pathRenderer)
{
    /// <summary>
    /// Renders a microformats mention link.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The resulting microformats string.</returns>
    public string Mention(User user)
    {
        const string format = """<a href="{0}" class="u-url mention" data-user="{1}">@{2}</a>""";

        return string.Format(
            format,
            user.RemoteId ?? pathRenderer.GetPathToActor(user),
            user.Id,
            user.Handle);
    }

    /// <summary>
    /// Renders a microformats hashtag link.
    /// </summary>
    /// <param name="hashtag">The hashtag.</param>
    /// <returns>The resulting microformats string.</returns>
    public string Hashtag(string hashtag)
    {
        const string format = """<a href="{0}" class="u-url hashtag">{1}</a>""";

        return string.Format(
            format,
            pathRenderer.GetPathToHashtag(hashtag),
            hashtag);
    }
}