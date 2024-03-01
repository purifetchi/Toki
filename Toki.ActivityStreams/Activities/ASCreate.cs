using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityStreams.Activities;

/// <summary>
/// An ActivityStreams create activity.
/// </summary>
public class ASCreate : ASActivity
{
    /// <summary>
    /// Constructs a new ASCreate activity.
    /// </summary>
    public ASCreate()
        : base("Create")
    {
        
    }
    
    /// <summary>
    /// The primary recipients of this note.
    /// </summary>
    [JsonPropertyName("to")]
    public IReadOnlyList<string>? To { get; set; }
    
    /// <summary>
    /// The secondary recipients of this note.
    /// </summary>
    [JsonPropertyName("cc")]
    public IReadOnlyList<string>? Cc { get; set; }
}