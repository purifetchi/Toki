using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.OAuth2;
using Toki.Binding;
using Toki.MastodonApi.Helpers;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Requests.Apps;
using Toki.MastodonApi.Schemas.Responses.Apps;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Apps;

/// <summary>
/// The controller for the "/api/v1/apps" route in Mastodon.
/// </summary>
[ApiController]
[Route("/api/v1/apps")]
[EnableCors("MastodonAPI")]
public class AppsController(
    OAuthManagementService managementService) : ControllerBase
{
    /// <summary>
    /// Sent by the client when they want to register a new oauth2 app.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>Either an application, or an error.</returns>
    [HttpPost]
    [Consumes("application/json", "application/x-www-form-urlencoded", "multipart/form-data")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateApp(
        [FromHybrid] CreateApplicationRequest request)
    {
        var uris = request.GetRedirectUrls();
        var uriValidation = MastodonOAuthHelper.ValidateRedirectUris(uris);
        
        if (uriValidation is not null)
            return UnprocessableEntity(uriValidation);

        var scopes = MastodonOAuthHelper.ExpandScopes(
            request.GetScopes());

        var app = await managementService.RegisterApp(
            request.ClientName,
            uris,
            scopes,
            request.Website);
        
        if (app is null)
            return BadRequest(new MastodonApiError("Couldn't register the application."));
        
        return Ok(new CreateApplicationResponse()
        {
            Id = $"{app.Id}",
            ClientName = app.ClientName,
            
            ClientId = app.ClientId,
            ClientSecret = app.ClientSecret,
            
            RedirectUri = request.RedirectUrls
        });
    }
}