using System.Text.Json.Serialization;

namespace Toki.ActivityPub.NodeInfo;

/// <summary>
/// The metadata for a node info response.
/// </summary>
public class NodeInfoMetadata
{
    /// <summary>
    /// The name of the instance.
    /// </summary>
    [JsonPropertyName("nodeName")]
    public string? Name { get; set; }
    
    /// <summary>
    /// The description of the node.
    /// </summary>
    [JsonPropertyName("nodeDescription")]
    public string? Description { get; set; }
}