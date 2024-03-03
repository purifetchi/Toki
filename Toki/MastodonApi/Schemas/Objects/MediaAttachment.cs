using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// A media attachment to a post.
/// </summary>
public record MediaAttachment
{
    /// <summary>
    /// The id of the status.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    
    /// <summary>
    /// The type of the attachment.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    
    /// <summary>
    /// The location of the original full-size attachment.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }
    
    /// <summary>
    /// The location of a scaled-down preview of the attachment.
    /// </summary>
    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; init; }
    
    /// <summary>
    /// Alternate text that describes what is in the media attachment, to be used for the visually impaired or when media attachments do not load.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}