using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Models;

namespace Toki.ActivityPub.Renderers;

/// <summary>
/// A class that helps with rendering paths from within the instance.
/// </summary>
/// <param name="opts">The instance configuration options.</param>
public class InstancePathRenderer(
    IOptions<InstanceConfiguration> opts)
{
    /// <summary>
    /// Gets the path to an actor from a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The path to the <see cref="Toki.ActivityStreams.Objects.ASActor"/> object on the server.</returns>
    public string GetPathToActor(User user) =>
        GetPathToActor(user.Handle);
    
    /// <summary>
    /// Gets the path to an actor from a handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>The path to the <see cref="Toki.ActivityStreams.Objects.ASActor"/> object on the server.</returns>
    public string GetPathToActor(string handle) =>
        $"https://{opts.Value.Domain}/users/{handle}";

    /// <summary>
    /// Gets the path to a note from a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The path to the <see cref="Toki.ActivityStreams.Objects.ASNote"/></returns>
    public string GetPathToPost(Post post) =>
        $"https://{opts.Value.Domain}/posts/{post.Id}";

    /// <summary>
    /// Returns a link to the default avatar.
    /// </summary>
    /// <returns>The default avatar.</returns>
    public string GetPathToDefaultAvatar() =>
        $"https://{opts.Value.Domain}/images/avatar.png";
    
    /// <summary>
    /// Returns a link to the default banner.
    /// </summary>
    /// <returns>The default banner.</returns>
    public string GetPathToDefaultBanner() =>
        $"https://{opts.Value.Domain}/images/banner.png";
    
    /// <summary>
    /// Gets the path to the hashtag.
    /// </summary>
    /// <param name="hashtag">The hashtag.</param>
    /// <returns>The path to it.</returns>
    public string GetPathToHashtag(string hashtag) =>
        $"https://{opts.Value.Domain}/tags/{hashtag[1..]}";
}