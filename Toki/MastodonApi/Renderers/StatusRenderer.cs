using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.ActivityPub.Renderers;
using Toki.Extensions;
using Toki.MastodonApi.Helpers;
using Toki.MastodonApi.Schemas.Objects;

namespace Toki.MastodonApi.Renderers;

/// <summary>
/// A renderer for Mastodon statuses.
/// </summary>
public class StatusRenderer(
    AccountRenderer accountRenderer,
    InstancePathRenderer pathRenderer,
    EmojiRepository emojiRepo,
    PostManagementService postManagementService)
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
    /// Renders the tags for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The tags.</returns>
    private IReadOnlyList<StatusTag>? RenderTagsFor(Post post)
    {
        return post.Tags?
            .Select(hashtag => new StatusTag
            {
                Name = hashtag[1..],
                Url = pathRenderer.GetPathToHashtag(hashtag)
            })
            .ToList();
    }

    /// <summary>
    /// Gets the emoji for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The emoji.</returns>
    private async Task<IReadOnlyList<CustomEmoji>> GetEmojiFor(Post post)
    {
        if (post.Emojis is null || post.Emojis.Count < 1)
            return [];
        
        var emojis = await emojiRepo.FindManyByIds(
            post.Emojis
                .Select(Ulid.Parse)
                .ToList());

        return emojis
            .Select(RenderEmojiFrom)
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
        Type = FileTypeDeduceHelper.GetFileType(postAttachment),

        Url = postAttachment.Url,
        PreviewUrl = postAttachment.Url,
        Description = postAttachment.Description,
    };

    /// <summary>
    /// Renders an emoji from the Toki <see cref="Emoji"/>.
    /// </summary>
    /// <param name="emoji">The emoji.</param>
    /// <returns>The mastodon <see cref="CustomEmoji"/></returns>
    public CustomEmoji RenderEmojiFrom(Emoji emoji) => new CustomEmoji
    {
        Shortcode = emoji.Shortcode.Trim(':'),
        Url = emoji.RemoteUrl,
        StaticUrl = emoji.RemoteUrl,
        Category = emoji.Category
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
            ConversationId = post.Context?.ToString(),
            
            Uri = url,
            Url = url,

            CreatedAt = post.CreatedAt,
            Content = post.Content,

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
            Mentions = RenderMentionsFor(post) ?? [],
            Tags = RenderTagsFor(post) ?? []
        };
    }

    /// <summary>
    /// Renders a status for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="post">The post to render the status from.</param>
    /// <returns>The resulting status.</returns>
    public async Task<Status> RenderStatusForUser(
        User? user,
        Post post)
    {
        if (user is null)
            return RenderForPost(post);
        
        var status = RenderForPost(post);
        var liked = await postManagementService.HasLiked(user, post);
        var boosted = await postManagementService.HasBoosted(user, post);
        var bookmarked = await postManagementService.HasBookmarked(user, post);
        
        status.Liked = liked;
        status.Boosted = boosted;
        status.Bookmarked = bookmarked;
        status.Emojis = await GetEmojiFor(post);
        
        return status;
    }

    /// <summary>
    /// Renders many statuses for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="posts">The posts to render from.</param>
    /// <returns>The statuses.</returns>
    public async Task<IReadOnlyList<Status>> RenderManyStatusesForUser(
        User? user,
        IList<Post> posts)
    {
        if (user is null)
            return posts.Select(RenderForPost).ToList();
        
        // Get all ids we're interested in (this also includes the IDs of boosted posts)
        var ids = posts
            .Concat(posts
                .Where(p => p.Boosting is not null)
                .Select(p => p.Boosting!))
            .Select(p => p.Id)
            .Distinct()
            .ToList();

        // Fetch both of these at once so we only do 2 DB hits while rendering many.
        var likes = await postManagementService.FindManyLikedPosts(user, ids);
        var boosts = await postManagementService.FindManyBoostedPosts(user, ids);
        var bookmarks = await postManagementService.FindManyBookmarkedPosts(user, ids);
        
        var results = new List<Status>();
        foreach (var post in posts)
        {
            var status = RenderForPost(post);
            
            // TODO: Preload all of the emojis at once.
            if (status.Boost != null)
            {
                status.Boost.Liked = likes.Contains(post.Boosting!.Id);
                status.Boost.Boosted = boosts.Contains(post.Boosting!.Id);
                status.Boost.Bookmarked = bookmarks.Contains(post.Boosting!.Id);
                status.Boost.Emojis = await GetEmojiFor(post.Boosting!);
            }
            else
            {
                status.Liked = likes.Contains(post.Id);
                status.Boosted = boosts.Contains(post.Id);
                status.Bookmarked = bookmarks.Contains(post.Id);
                status.Emojis = await GetEmojiFor(post);
            }
            
            results.Add(status);
        }

        return results;
    }
}