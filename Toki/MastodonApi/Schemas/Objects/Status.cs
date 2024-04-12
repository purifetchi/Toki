using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// A Mastodon status.
/// </summary>
public record Status
{
    /// <summary>
    /// The id of the status.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    
    /// <summary>
    /// URI of the status used for federation.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; init; }
    
    /// <summary>
    /// When was this status created?
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Timestamp of when the status was last edited.
    /// </summary>
    [JsonPropertyName("edited_at")]
    public DateTimeOffset? EditedAt { get; init; } = null;
    
    /// <summary>
    /// The account that authored this status.
    /// </summary>
    [JsonPropertyName("account")]
    public Account? Account { get; init; }
    
    /// <summary>
    /// HTML-encoded status content.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; init; }
    
    /// <summary>
    /// Visibility of this status.
    /// </summary>
    [JsonPropertyName("visibility")]
    public string? Visibility { get; init; }
    
    /// <summary>
    /// Is this status marked as sensitive content?
    /// </summary>
    [JsonPropertyName("sensitive")]
    public bool Sensitive { get; init; }
    
    /// <summary>
    /// Subject or summary line, below which status content is collapsed until expanded.
    /// </summary>
    [JsonPropertyName("spoiler_text")]
    public string? SpoilerText { get; init; }
    
    /// <summary>
    /// How many boosts this status has received.
    /// </summary>
    [JsonPropertyName("reblogs_count")]
    public int BoostCount { get; init; }
    
    /// <summary>
    /// How many favourites this status has received.
    /// </summary>
    [JsonPropertyName("favourites_count")]
    public int FavouritesCount { get; init; }
    
    /// <summary>
    /// How many replies this status has received.
    /// </summary>
    [JsonPropertyName("replies_count")]
    public int RepliesCount { get; init; }
    
    /// <summary>
    /// ID of the status being replied to.
    /// </summary>
    [JsonPropertyName("in_reply_to_id")]
    public string? InReplyToId { get; init; }
    
    /// <summary>
    /// ID of the account that authored the status being replied to.
    /// </summary>
    [JsonPropertyName("in_reply_to_account_id")]
    public string? InReplyToAccountId { get; init; }
    
    /// <summary>
    /// Media that is attached to this status.
    /// </summary>
    [JsonPropertyName("media_attachments")]
    public IReadOnlyList<MediaAttachment>? Attachments { get; init; }
    
    /// <summary>
    /// The boosted post.
    /// </summary>
    [JsonPropertyName("reblog")]
    public Status? Boost { get; init; }

    /// <summary>
    /// The emojis in this post.
    /// </summary>
    [JsonPropertyName("emojis")]
    public IReadOnlyList<CustomEmoji> Emojis { get; set; } = [];
    
    /// <summary>
    /// The emojis in this post.
    /// </summary>
    [JsonPropertyName("mentions")]
    public IReadOnlyList<Mention> Mentions { get; init; } = [];

    /// <summary>
    /// Hashtags used within the status content.
    /// </summary>
    [JsonPropertyName("tags")]
    public IReadOnlyList<StatusTag> Tags { get; init; } = [];
    
    /// <summary>
    /// The url to the HTML representation of the post.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }
    
    /// <summary>
    /// Have you liked this status?
    /// </summary>
    [JsonPropertyName("favourited")]
    public bool? Liked { get; set; }
    
    /// <summary>
    /// Have you boosted this status?
    /// </summary>
    [JsonPropertyName("reblogged")]
    public bool? Boosted { get; set; }
    
    /// <summary>
    /// Have you pinned this status?
    /// </summary>
    [JsonPropertyName("pinned")]
    public bool? Pinned { get; set; }
    
    /// <summary>
    /// The ID of the conversation. (Used in akkoma-fe, PROBABLY not a Mastodon thing).
    /// </summary>
    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("card")]
    public object? card { get; set; } = null;
    [JsonPropertyName("language")]
    public string? language { get; set; } = null;
    [JsonPropertyName("text")]
    public string? text { get; set; } = null;
    [JsonPropertyName("poll")]
    public object? poll { get; set; } = null;
    [JsonPropertyName("bookmarked")]
    public bool bookmarked { get; set; } = false;

    // TODO: The rest of the status.
}