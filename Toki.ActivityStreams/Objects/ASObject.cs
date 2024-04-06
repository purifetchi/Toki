using System.Text.Json.Serialization;
using Toki.ActivityStreams.Context;
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
    /// The context.
    /// </summary>
    [JsonPropertyName("@context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-999)]
    public LdContext? Context { get; set; } = LdContext.Default;
    
    /// <summary>
    /// The ID of the object.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    /// <summary>
    /// The type of this object.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    
    /// <summary>
    /// When was this object published at?
    /// </summary>
    [JsonPropertyName("published")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>
    /// Is this ASObject resolved?
    /// </summary>
    [JsonIgnore] public bool IsResolved => Type != null && GetType() != typeof(ASObject);

    /// <summary>
    /// Creates a link to an ASObject.
    /// </summary>
    /// <param name="id">The id to link to.</param>
    /// <returns>The link ASObject.</returns>
    public static ASObject Link(string id)
    {
        return new ASObject
        {
            Id = id
        };
    }
}