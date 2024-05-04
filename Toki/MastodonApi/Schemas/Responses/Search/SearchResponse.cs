using System.Text.Json.Serialization;
using Toki.MastodonApi.Schemas.Objects;

namespace Toki.MastodonApi.Schemas.Responses.Search;

public record SearchResponse
{
    /// <summary>
    /// The list of resulting accounts.
    /// </summary>
    [JsonPropertyName("accounts")]
    public IReadOnlyList<Account> Accounts { get; init; } = [];
    
    /// <summary>
    /// The list of resulting posts.
    /// </summary>
    [JsonPropertyName("statuses")]
    public IReadOnlyList<Status> Statuses { get; init; } = [];
    
    /// <summary>
    /// The list of resulting hashtags.
    /// </summary>
    // TODO: This is a stub. Implement that when we'll have proper hashtags.
    [JsonPropertyName("hashtags")]
    public IReadOnlyList<object> Hashtags { get; init; } = [];
}