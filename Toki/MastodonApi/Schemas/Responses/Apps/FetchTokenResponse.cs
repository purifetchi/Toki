using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Responses.Apps;

/// <summary>
/// The response to the token fetch request.
/// </summary>
public record FetchTokenResponse
{
    /// <summary>
    /// The access token.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    /// <summary>
    /// The type of the token.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; } = "Bearer";
    
    /// <summary>
    /// The scopes.
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }
    
    /// <summary>
    /// When was this token created?
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; init; }
}