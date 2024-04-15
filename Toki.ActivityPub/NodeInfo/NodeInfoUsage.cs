using System.Text.Json.Serialization;

namespace Toki.ActivityPub.NodeInfo;

/// <summary>
/// Usage statistics for this node.
/// </summary>
public record NodeInfoUsage
{
    /// <summary>
    /// The usage statistics of this node's users.
    /// </summary>
    [JsonPropertyName("users")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NodeInfoUsageUsers? Users { get; init; }
    
    /// <summary>
    /// The amount of local posts.
    /// </summary>
    [JsonPropertyName("localPosts")]
    public int? LocalPosts { get; init; }
}