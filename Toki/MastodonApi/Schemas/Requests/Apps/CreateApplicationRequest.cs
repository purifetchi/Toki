using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Toki.MastodonApi.Schemas.Requests.Apps;

/// <summary>
/// The request sent for when an end client wants to register an oauth app.
/// </summary>
public record CreateApplicationRequest
{
    /// <summary>
    /// The name of the client.
    /// </summary>
    [BindProperty(Name = "client_name")]
    [JsonPropertyName("client_name")]
    public required string ClientName { get; init; }
    
    /// <summary>
    /// The name of the client.
    /// </summary>
    [BindProperty(Name = "redirect_uris")] 
    [JsonPropertyName("redirect_uris")]
    public required string RedirectUrls { get; init; }

    /// <summary>
    /// The space separated list of scopes.
    /// </summary>
    [BindProperty(Name = "scopes")]
    [JsonPropertyName("scopes")]
    public string? Scopes { get; init; }
    
    /// <summary>
    /// The website of the app.
    /// </summary>
    [BindProperty(Name = "website")]
    [JsonPropertyName("website")]
    public string? Website { get; init; }

    /// <summary>
    /// Gets the scopes for this request.
    /// </summary>
    /// <returns>The scopes.</returns>
    public List<string> GetScopes() =>
        Scopes?.Split(' ').ToList() ?? ["read"];

    /// <summary>
    /// Gets the list of redirect urls for this request.
    /// </summary>
    /// <returns></returns>
    public List<string> GetRedirectUrls() =>
        RedirectUrls.Split(' ').ToList();
}
