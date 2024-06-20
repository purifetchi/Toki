using System.Text.Json.Serialization;
using Toki.ActivityPub.WebFinger;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityPub.NodeInfo;

/// <summary>
/// The version selector response for node info.
/// </summary>
public class NodeInfoVersionSelectorResponse
{
    /// <summary>
    /// The list of links.
    /// </summary>
    [JsonPropertyName("links")]
    [JsonConverter(typeof(ListOrObjectConverter<WebFingerLink>))]
    public IReadOnlyList<WebFingerLink>? Links { get; set; } // TODO: This should probably use its own class, but it's 1:1 with the WebFinger response.
}