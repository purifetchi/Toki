using System.Text.Json.Serialization;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityStreams.Activities;

/// <summary>
/// An ActivityStreams activity.
/// </summary>
public class ASActivity : ASObject
{
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
}