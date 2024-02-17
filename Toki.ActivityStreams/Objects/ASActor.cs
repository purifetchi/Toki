using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams actor.
/// </summary>
public class ASActor : ASObject
{
    /// <summary>
    /// The name of the actor.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}