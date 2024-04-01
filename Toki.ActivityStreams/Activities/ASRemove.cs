using System.Text.Json.Serialization;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityStreams.Activities;

/// <summary>
/// An ActivityStreams remove activity.
/// </summary>
public class ASRemove : ASActivity
{
    /// <summary>
    /// Constructs a new ASRemove activity.
    /// </summary>
    public ASRemove()
        : base("Remove")
    {
        
    }
    
    /// <summary>
    /// The target collection.
    /// </summary>
    [JsonPropertyName("target")]
    public ASObject? Target { get; set; }
}