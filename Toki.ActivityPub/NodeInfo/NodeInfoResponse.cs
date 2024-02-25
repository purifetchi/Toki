using System.Text.Json.Serialization;

namespace Toki.ActivityPub.NodeInfo;

/// <summary>
/// A node info response.
/// </summary>
public class NodeInfoResponse
{
    /// <summary>
    /// The version of the node info response.
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; } = "2.1";
    
    /// <summary>
    /// The software powering this node.
    /// </summary>
    [JsonPropertyName("software")]
    public NodeInfoSoftware? Software { get; init; }
    
    /// <summary>
    /// The protocols supported by this node.
    /// </summary>
    [JsonPropertyName("protocols")]
    public IReadOnlyList<string>? Protocols { get; init; }

    // TODO: Services
    
    /// <summary>
    /// Does this node have open registrations?
    /// </summary>
    [JsonPropertyName("openRegistrations")]
    public bool OpenRegistrations { get; init; } = false;
    
    // TODO: Usage
    
    /// <summary>
    /// The metadata of this instance.
    /// </summary>
    [JsonPropertyName("metadata")]
    public NodeInfoMetadata? Metadata { get; init; }
}