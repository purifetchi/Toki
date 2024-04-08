using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams emoji.
/// </summary>
public class ASEmoji : ASLink
{
    /// <summary>
    /// The emoji image.
    /// </summary>
    [JsonPropertyName("icon")]
    public ASImage? Icon { get; set; }
    
    /// <summary>
    /// The link to the emoji.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// The name of the emoji.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}