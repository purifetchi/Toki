using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Extensions;

/// <summary>
/// Extensions for the <see cref="ASNote"/> class.
/// </summary>
public static class ASNoteExtensions
{
    /// <summary>
    /// Gets the post visibility for a note.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <param name="user">The user who posted it.</param>
    /// <returns>The post visibility.</returns>
    public static PostVisibility GetPostVisibility(
        this ASNote note, 
        User user)
    {
        if (note.To?.Any(IsPublicUri) == true)
            return PostVisibility.Public;

        if (note.Cc?.Any(IsPublicUri) == true)
            return PostVisibility.Unlisted;
        
        // TODO: We need a better way of getting the followers of an actor
        //       as the followers URI isn't always {actor}/followers.
        if (note.To?.Any(uri => uri == $"{user.RemoteId}/followers") == true)
            return PostVisibility.Followers;
        
        return PostVisibility.Direct;
    }

    /// <summary>
    /// Is the given URI an ActivityStreams public URI?
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <returns>Whether it is a public URI.</returns>
    private static bool IsPublicUri(string uri) => uri switch
    {
        "https://www.w3.org/ns/activitystreams#Public" => true,
        "as:Public" => true, // Thanks to @Oneric@akko.wtf for the tip on this one
        "Public" => true,
        _ => false
    };
}