using Toki.ActivityPub.Models;
using Toki.ActivityPub.Renderers;
using Toki.Extensions;
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
    /// Renders the attachments for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The attachments.</returns>
    private IReadOnlyList<MediaAttachment>? RenderAttachmentsFor(Post post)
    {
        return post.Attachments?
            .Select(RenderAttachmentFrom)
            .ToList();
    }

    /// <summary>
    /// Renders the mentions for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The mentions.</returns>
    private IReadOnlyList<Mention>? RenderMentionsFor(Post post)
    {
        return post.UserMentions?
            .Select(m => new Mention
            {
                Id = m.Id,
                Url = m.Url,
                Username = m.Handle,
                WebFingerResource = m.Handle
            })
            .ToList();
    }

    /// <summary>
    /// Renders a media attachment from the Toki <see cref="PostAttachment"/>.
    /// </summary>
    /// <param name="postAttachment">The post attachment.</param>
    /// <returns>The resulting media attachment.</returns>
    public MediaAttachment RenderAttachmentFrom(PostAttachment postAttachment) => new MediaAttachment
    {
        Id = $"{postAttachment.Id}",
        Type = "image", // TODO: Deduce this based on the MIME

        Url = postAttachment.Url,
        PreviewUrl = postAttachment.Url,
        Description = postAttachment.Description,
    };
    
    /// <summary>
    /// Renders a status for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The resulting status.</returns>
    public Status RenderForPost(Post post)
    {
        var url = post.RemoteId ??
                  pathRenderer.GetPathToPost(post);
        
        return new Status
        {
            Id = $"{post.Id}",
            
            Uri = url,
            Url = url,

            CreatedAt = post.CreatedAt,
            Content = post.Content,

            // TODO: Caching in the account renderer path.
            Account = accountRenderer.RenderAccountFrom(post.Author),

            BoostCount = post.BoostCount,
            FavouritesCount = post.LikeCount,

            Sensitive = post.Sensitive,
            SpoilerText = post.ContentWarning ?? "",

            Visibility = post.Visibility
                .ToMastodonString(),
            
            Boost = post.Boosting is not null ?
                RenderForPost(post.Boosting) :
                null,
            
            InReplyToId = post.ParentId?.ToString(),
            InReplyToAccountId = post.Parent?.AuthorId.ToString(),

            Attachments = RenderAttachmentsFor(post) ?? [],
            Mentions = RenderMentionsFor(post) ?? []
        };
    }
}