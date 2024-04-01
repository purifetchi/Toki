using System.Text.Json.Serialization;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityStreams.Activities;

/// <summary>
/// An ActivityStreams add activity.
/// </summary>
public class ASAdd : ASActivity
{
    /// <summary>
    /// Constructs a new ASAdd activity.
    /// </summary>
    public ASAdd()
        : base("Add")
    {
        
    }
    
    /// <summary>
    /// The target collection.
    /// </summary>
    [JsonPropertyName("target")]
    public ASObject? Target { get; set; }
}