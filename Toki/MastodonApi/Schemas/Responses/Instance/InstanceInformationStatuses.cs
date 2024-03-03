using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Responses.Instance;

/// <summary>
/// Information about statuses.
/// </summary>
public record InstanceInformationStatuses
{
    // TODO: Make this all configurable, of course.
    
    /// <summary>
    /// The maximum amount of characters per post.
    /// </summary>
    [JsonPropertyName("max_characters")]
    public string? MaxCharacters { get; set; } = "500";
    
    /// <summary>
    /// The maximum amount of media attachments per post.
    /// </summary>
    [JsonPropertyName("max_media_attachments")]
    public string? MaxMediaAttachments { get; set; } = "0"; // TODO: Support media attachments
    
    /// <summary>
    /// The amount of characters that are reserved for every url.
    /// </summary>
    [JsonPropertyName("characters_reserved_per_url")]
    public string? CharactersReservedPerUrl { get; set; } = "23";
}