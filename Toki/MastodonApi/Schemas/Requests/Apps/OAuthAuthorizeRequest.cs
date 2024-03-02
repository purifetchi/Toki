using Microsoft.AspNetCore.Mvc;

namespace Toki.MastodonApi.Schemas.Requests.Apps;

/// <summary>
/// The OAuth authorization request.
/// </summary>
public record OAuthAuthorizeRequest
{
    /// <summary>
    /// The response type of this request.
    /// </summary>
    [BindProperty(Name = "response_type", SupportsGet = true)]
    public required string ResponseType { get; set; }
    
    /// <summary>
    /// The id of the client app.
    /// </summary>
    [BindProperty(Name = "client_id", SupportsGet = true)]
    public required string ClientId { get; set; }
    
    /// <summary>
    /// The uri of the site we're going to redirect to.
    /// </summary>
    [BindProperty(Name = "redirect_uri", SupportsGet = true)]
    public required string RedirectUri { get; set; }

    /// <summary>
    /// The scopes
    /// </summary>
    [BindProperty(Name = "scope", SupportsGet = true)]
    public string? Scope { get; set; } = "read";
}