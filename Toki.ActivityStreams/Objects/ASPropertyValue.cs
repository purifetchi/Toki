using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams key-value object.
/// </summary>
public class ASPropertyValue : ASLink
{
    /// <summary>
    /// The name of the property.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// The value of the property.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}