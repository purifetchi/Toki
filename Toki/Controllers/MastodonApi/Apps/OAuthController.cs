using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.OAuth2;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Requests.Apps;
using Toki.MastodonApi.Schemas.Responses.Apps;

namespace Toki.Controllers.MastodonApi.Apps;

/// <summary>
/// The "/oauth" controller for Mastodon api.
/// </summary>
[ApiController]
[Route("/oauth")]
[EnableCors("MastodonAPI")]
public class OAuthController(
    OAuthManagementService managementService,
    OAuthRepository repo) : ControllerBase
{
    /// <summary>
    /// Sent by a client when they want to retrieve a token from an auth code.
    /// </summary>
    /// <param name="request">The request sent in.</param>
    /// <returns>The token response, or an error.</returns>
    [HttpPost]
    [Route("token")]
    [Produces("application/json")]
    public async Task<IActionResult> FetchToken(
        [FromForm] FetchTokenRequest request)
    {
        var app = await repo.FindByClientId(request.ClientId);
        if (app is null || app.ClientSecret != request.ClientSecret)
            return Unauthorized(new MastodonApiError("invalid_client"));

        if (request.Code is null)
            return Unauthorized(new MastodonApiError("invalid_code"));
        
        var token = await repo.FindTokenByAuthCode(request.Code);
        if (token is null)
            return Unauthorized(new MastodonApiError("invalid_code"));

        if (token.Active)
            return BadRequest(new MastodonApiError("Authorization grant is expired or malformed."));

        await managementService.ActivateToken(token);
            
        return Ok(new FetchTokenResponse()
        {
            AccessToken = token.Token,
            Scope = string.Join(" ", token.Scopes!),
            
            CreatedAt = token.CreatedAt.ToUnixTimeSeconds()
        });
    }
}