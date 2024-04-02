using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Toki.MastodonApi.Schemas.Requests.Apps;

/// <summary>
/// The request for when we're fetching an OAuth2 token.
/// </summary>
public record RevokeTokenRequest
{
    /// <summary>
    /// The client id.
    /// </summary>
    [BindProperty(Name = "client_id")]
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }
    
    /// <summary>
    /// The client secret.
    /// </summary>
    [BindProperty(Name = "client_secret")]
    [JsonPropertyName("client_secret")]
    public required string ClientSecret { get; init; }
    
    /// <summary>
    /// The previously obtained token, to be invalidated.
    /// </summary>
    [BindProperty(Name = "token")]
    [JsonPropertyName("token")]
    public required string Token { get; init; }
}