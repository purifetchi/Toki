using System.Text.Json.Serialization;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityStreams.Activities.Extensions;

/// <summary>
/// An ActivityStreams Bite activity. (Extension)
/// </summary>
public class ASBite : ASActivity
{
    /// <summary>
    /// Creates a new ASBite activity.
    /// </summary>
    protected ASBite()
        : base("Bite")
    {
        
    }
    
    /// <summary>
    /// The target being bit.
    /// </summary>
    [JsonPropertyName("target")]
    public ASObject? Target { get; set; }
}