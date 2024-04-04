using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Responses.Instance;

/// <summary>
/// Statistics about how much information the instance contains.
/// </summary>
public record InstanceInformationStatistics
{
    /// <summary>
    /// Total users on this instance.
    /// </summary>
    [JsonPropertyName("user_count")]
    public int UserCount { get; init; }
    
    /// <summary>
    /// Total statuses on this instance.
    /// </summary>
    [JsonPropertyName("status_count")]
    public int StatusCount { get; init; }
    
    /// <summary>
    /// Total domains discovered by this instance.
    /// </summary>
    [JsonPropertyName("domain_count")]
    public int DomainCount { get; init; }
}