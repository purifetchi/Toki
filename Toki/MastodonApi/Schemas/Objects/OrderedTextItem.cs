using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// An object of type {"id": "xyz", "text": "abcd"}
/// </summary>
public class OrderedTextItem
{
    /// <summary>
    /// The id.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The text.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}