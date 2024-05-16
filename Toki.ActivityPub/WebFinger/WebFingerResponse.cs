using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityPub.WebFinger;

/// <summary>
/// The default WebFinger response.
/// </summary>
public record WebFingerResponse
{
    /// <summary>
    /// The subject of the WebFinger response.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; init; } = null!;

    /// <summary>
    /// The aliases of said user.
    /// </summary>
    [JsonPropertyName("aliases")]
    public IReadOnlyList<string> Aliases { get; init; } = null!;

    /// <summary>
    /// The links to this user.
    /// </summary>
    [JsonPropertyName("links")]
    [JsonConverter(typeof(ListOrObjectConverter<WebFingerLink>))]
    public IReadOnlyList<WebFingerLink> Links { get; init; } = null!;
}