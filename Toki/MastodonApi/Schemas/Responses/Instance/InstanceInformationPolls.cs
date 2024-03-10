using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Responses.Instance;

/// <summary>
/// The information about polls.
/// </summary>
public record InstanceInformationPolls
{
    /// <summary>
    /// The maximum amount of options.
    /// </summary>
    [JsonPropertyName("max_options")]
    public int MaxOptions { get; init; }
    
    /// <summary>
    /// The maximum amount of characters per option.
    /// </summary>
    [JsonPropertyName("max_characters_per_option")]
    public int MaxCharactersPerOption { get; init; }
    
    /// <summary>
    /// The minimum amount of time for the poll to expire.
    /// </summary>
    [JsonPropertyName("min_expiration")]
    public int MinExpiration { get; init; }
    
    /// <summary>
    /// The maximum amount of time for the poll to expire.
    /// </summary>
    [JsonPropertyName("max_expiration")]
    public int MaxExpiration { get; init; }
}