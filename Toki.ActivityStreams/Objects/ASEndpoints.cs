using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// The ActivityStreams endpoints.
/// </summary>
public class ASEndpoints
{
    /// <summary>
    /// The shared inbox of this instance.
    /// </summary>
    [JsonPropertyName("sharedInbox")]
    public ASObject? SharedInbox { get; set; }
}