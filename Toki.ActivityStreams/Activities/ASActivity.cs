using System.Text.Json.Serialization;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityStreams.Activities;

/// <summary>
/// An ActivityStreams activity.
/// </summary>
public abstract class ASActivity : ASObject
{
    /// <summary>
    /// Constructs a new ASActivity of a given type.
    /// </summary>
    /// <param name="type">The type.</param>
    protected ASActivity(string type)
        : base(type)
    {
        
    }
    
    /// <summary>
    /// The actor that caused this activity.
    /// </summary>
    [JsonPropertyName("actor")]
    public required ASObject Actor { get; init; }
    
    /// <summary>
    /// The object this activity has done.
    ///
    /// NOTE: This isn't always present, especially for IntransitiveActivities.
    /// </summary>
    [JsonPropertyName("object")]
    public ASObject? Object { get; init; }
    
    /// <summary>
    /// The primary recipients of this note.
    /// </summary>
    [JsonPropertyName("to")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? To { get; set; }
    
    /// <summary>
    /// The secondary recipients of this note.
    /// </summary>
    [JsonPropertyName("cc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Cc { get; set; }
}