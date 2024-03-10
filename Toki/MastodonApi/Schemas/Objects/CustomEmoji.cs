using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// Represents a custom emoji.
/// </summary>
public record CustomEmoji
{
    /// <summary>
    /// The shortcode.
    /// </summary>
    [JsonPropertyName("shortcode")]
    public required string Shortcode { get; init; }
    
    /// <summary>
    /// A link to the custom emoji.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }
    
    /// <summary>
    /// A link to a static copy of the custom emoji.
    /// </summary>
    [JsonPropertyName("static_url")]
    public string? StaticUrl { get; init; }

    /// <summary>
    /// Whether this Emoji should be visible in the picker or unlisted.
    /// </summary>
    [JsonPropertyName("visible_in_picker")]
    public bool VisibleInPicker { get; init; } = true;
    
    /// <summary>
    /// Used for sorting custom emoji in the picker.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; init; }
}