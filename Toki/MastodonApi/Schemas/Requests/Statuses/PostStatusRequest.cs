using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Models.Enums;

namespace Toki.MastodonApi.Schemas.Requests.Statuses;

/// <summary>
/// A request to post a status.
/// </summary>
public record PostStatusRequest
{
    /// <summary>
    /// The text content of the status.
    /// </summary>
    [JsonPropertyName("status")]
    [BindProperty(Name = "status")]
    public string? Status { get; init; }
    
    /// <summary>
    /// Include Attachment IDs to be attached as media.
    /// </summary>
    [JsonPropertyName("media_ids")]
    [BindProperty(Name = "media_ids")]
    public IReadOnlyList<string>? MediaIds { get; init; }
    
    // TODO: Polls
    
    /// <summary>
    /// ID of the status being replied to, if status is a reply.
    /// </summary>
    [JsonPropertyName("in_reply_to_id")]
    [BindProperty(Name = "in_reply_to_id")]
    public string? InReplyTo { get; init; }
    
    /// <summary>
    /// Mark status and attached media as sensitive?
    /// </summary>
    [JsonPropertyName("sensitive")]
    [BindProperty(Name = "sensitive")]
    public bool Sensitive { get; init; }
    
    /// <summary>
    /// Text to be shown as a warning or subject before the actual content.
    /// </summary>
    [JsonPropertyName("spoiler_text")]
    [BindProperty(Name = "spoiler_text")]
    public string? SpoilerText { get; init; }
    
    /// <summary>
    /// Sets the visibility of the posted status.
    /// </summary>
    [JsonPropertyName("visibility")]
    [BindProperty(Name = "visibility")]
    public string? Visibility { get; init; }
    
    /// <summary>
    /// Gets the visibility for this post.
    /// </summary>
    /// <returns>The <see cref="PostVisibility"/></returns>
    public PostVisibility GetVisibility() => Visibility switch
    {
        "public" => PostVisibility.Public,
        "unlisted" => PostVisibility.Unlisted,
        "private" => PostVisibility.Followers,
        "direct" => PostVisibility.Direct,
        _ => PostVisibility.Public
    };
    
    // TODO: language, scheduled_at
}