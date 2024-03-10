using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// A mention in a status.
/// </summary>
public record Mention
{
    /// <summary>
    /// The account ID of the mentioned user.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    /// <summary>
    /// The username of the mentioned user.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }
    
    /// <summary>
    /// The location of the mentioned user’s profile.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }
    
    /// <summary>
    /// The webfinger acct: URI of the mentioned user. Equivalent to username for local users, or username@domain for remote users.
    /// </summary>
    [JsonPropertyName("acct")]
    public string? WebFingerResource { get; init; }
}