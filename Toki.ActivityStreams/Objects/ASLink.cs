using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams link.
/// </summary>
[JsonConverter(typeof(ASLinkConverter))]
public class ASLink
{
    /// <summary>
    /// The type of the link
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}