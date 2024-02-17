using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams note.
/// </summary>
public class ASNote : ASObject
{
    /// <summary>
    /// The content of the note.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}