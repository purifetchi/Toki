using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// A hashtag on the post.
/// </summary>
public record StatusTag
{
    /// <summary>
    /// The value of the hashtag after the # sign.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    
    /// <summary>
    /// A link to the hashtag on the instance.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}