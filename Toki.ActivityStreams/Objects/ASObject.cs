using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams object.
/// </summary>
[JsonConverter(typeof(ASObjectConverter))]
public class ASObject
{
    /// <summary>
    /// The default ASObject constructor.
    /// </summary>
    public ASObject()
    {
        
    }

    /// <summary>
    /// Constructs a new ASObject with a given type.
    /// </summary>
    /// <param name="type">The type.</param>
    public ASObject(string type)
    {
        Type = type;
    }
    
    /// <summary>
    /// The ID of the object.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    /// <summary>
    /// The type of this object.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Is this ASObject resolved?
    /// </summary>
    [JsonIgnore] public bool IsResolved => Type != null && GetType() != typeof(ASObject);
}