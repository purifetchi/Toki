using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams mention.
/// </summary>
public class ASMention : ASLink
{
    /// <summary>
    /// The name of the actor we're mentioning.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// The link to the actor.
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; set; }
}