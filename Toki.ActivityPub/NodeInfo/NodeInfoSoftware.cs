using System.Text.Json.Serialization;

namespace Toki.ActivityPub.NodeInfo;

/// <summary>
/// The node info software.
/// </summary>
public class NodeInfoSoftware
{
    /// <summary>
    /// The name of the software.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    /// <summary>
    /// The version of the software.
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }
    
    /// <summary>
    /// The repository of the software.
    /// </summary>
    [JsonPropertyName("repository")]
    public string? Repository { get; init; }
    
    /// <summary>
    /// The homepage of the software.
    /// </summary>
    [JsonPropertyName("homepage")]
    public string? Homepage { get; init; }
}