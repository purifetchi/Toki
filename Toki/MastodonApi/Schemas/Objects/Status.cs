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
    
    // TODO: The rest of the status.
}