using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams image.
/// </summary>
public class ASImage : ASLink
{
    /// <summary>
    /// The url of the image.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}