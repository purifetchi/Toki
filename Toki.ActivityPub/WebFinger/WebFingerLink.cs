using System.Text.Json.Serialization;

namespace Toki.ActivityPub.WebFinger;

/// <summary>
/// A single WebFinger link.
/// </summary>
public record WebFingerLink
{
    /// <summary>
    /// What this link is relative to?
    /// </summary>
    [JsonPropertyName("rel")]
    public string Relative { get; init; } = null!;
    
    /// <summary>
    /// The MIME type of this link.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// The hyperlink to this object.
    /// </summary>
    [JsonPropertyName("href")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Hyperlink { get; init; }
    
    /// <summary>
    /// The template this link is pointing to.
    /// </summary>
    [JsonPropertyName("template")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Template { get; init; }
}