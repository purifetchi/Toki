using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Toki.MastodonApi.Schemas.Requests.Apps;

/// <summary>
/// The request for when we're fetching an OAuth2 token.
/// </summary>
public record FetchTokenRequest
{
    /// <summary>
    /// The grant type.
    /// </summary>
    [BindProperty(Name = "grant_type")]
    [JsonPropertyName("grant_type")]
    public required string GrantType { get; init; }
    
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
    /// The redirect URI.
    /// </summary>
    [BindProperty(Name = "redirect_uri")]
    [JsonPropertyName("redirect_uri")]
    public string? RedirectUri { get; init; }
    
    /// <summary>
    /// The username.
    /// </summary>
    [BindProperty(Name = "username")]
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    
    /// <summary>
    /// The password.
    /// </summary>
    [BindProperty(Name = "password")]
    [JsonPropertyName("password")]
    public string? Password { get; set; }
    
    /// <summary>
    /// The user authorization code.
    /// </summary>
    [BindProperty(Name = "code")]
    [JsonPropertyName("code")]
    public string? Code { get; init; }
    
    // TODO: Token level scopes(???) No clue how it works like, or how it differs
    //       from what we receive thru /oauth/authorize.
}