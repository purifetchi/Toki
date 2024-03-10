using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// A field.
/// </summary>
public record Field
{
    /// <summary>
    /// The key of a given field’s key-value pair.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// The value associated with the name key.
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; set; }
    
    /// <summary>
    /// Timestamp of when the server verified a URL value for a rel=“me” link.
    /// </summary>
    [JsonPropertyName("verified_at")]
    public DateTimeOffset? VerifiedAt { get; set; }
}