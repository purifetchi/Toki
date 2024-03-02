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
    public required string ClientName { get; init; }
    
    /// <summary>
    /// The name of the client.
    /// </summary>
    [BindProperty(Name = "redirect_uris")] 
    public required string RedirectUrls { get; init; }

    /// <summary>
    /// The space separated list of scopes.
    /// </summary>
    [BindProperty(Name = "scopes")]
    public string? Scopes { get; init; } = "read";
    
    /// <summary>
    /// The website of the app.
    /// </summary>
    [BindProperty(Name = "website")]
    public string? Website { get; init; }
}
