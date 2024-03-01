using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;

namespace Toki.ActivityPub.Extensions;

/// <summary>
/// Extensions for the <see cref="Post"/> model.
/// </summary>
public static class PostExtensions
{
    /// <summary>
    /// Returns whether this post is a quote post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>Whether it is a quote post.</returns>
    public static bool IsQuote(this Post post) =>
        post.Boosting is not null && post.Content is not null;

    /// <summary>
    /// Returns whether this post can be boosted.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>Whether it can be boosted.</returns>
    public static bool CanBeBoosted(this Post post) =>
        (post.Boosting is null || (post.Boosting is null && post.Content is not null)) && 
        (post.Visibility is PostVisibility.Public or PostVisibility.Unlisted);
}