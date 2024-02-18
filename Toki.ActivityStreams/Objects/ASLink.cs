using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams link.
/// </summary>
public class ASLink
{
    /// <summary>
    /// The type of the link
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}