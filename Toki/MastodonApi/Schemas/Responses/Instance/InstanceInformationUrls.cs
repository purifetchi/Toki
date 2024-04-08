using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Responses.Instance;

/// <summary>
/// URLs of interest for clients apps.
/// </summary>
public record InstanceInformationUrls
{
    /// <summary>
    /// The Websockets URL for connecting to the streaming API.
    /// </summary>
    [JsonPropertyName("streaming_api")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StreamingApi { get; init; } = null;
}