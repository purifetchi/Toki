using Toki.ActivityPub.Models;
using Toki.ActivityPub.Renderers;
using Toki.MastodonApi.Schemas.Objects;

namespace Toki.MastodonApi.Renderers;

/// <summary>
/// A renderer for Mastodon statuses.
/// </summary>
public class StatusRenderer(
    AccountRenderer accountRenderer,
    InstancePathRenderer pathRenderer)
{
    /// <summary>
    /// Renders a status for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The resulting status.</returns>
    public Status RenderForPost(Post post)
    {
        return new Status
        {
            Id = $"{post.Id}",
            
            Uri = post.RemoteId ?? 
                  pathRenderer.GetPathToPost(post),

            CreatedAt = post.CreatedAt,
            Content = post.Content,

            // TODO: Caching in the account renderer path.
            Account = accountRenderer.RenderAccountFrom(post.Author),

            BoostCount = post.BoostCount,
            FavouritesCount = post.LikeCount,

            Sensitive = post.Sensitive,
            SpoilerText = post.ContentWarning
        };
    }
}