using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Models.OAuth;
using Toki.ActivityPub.OAuth2;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Users;
using Toki.Binding;
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
    OAuthRepository repo,
    UserSessionService sessionService,
    IOptions<InstanceConfiguration> opts) : ControllerBase
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
        [FromHybrid] FetchTokenRequest request)
    {
        var app = await repo.FindByClientId(request.ClientId);
        if (app is null || app.ClientSecret != request.ClientSecret)
            return Unauthorized(new MastodonApiError("invalid_client"));

        OAuthToken? token;
        switch (request.GrantType)
        {
            case "authorization_code":
                if (request.Code is null)
                    return Unauthorized(new MastodonApiError("invalid_code"));
        
                token = await repo.FindTokenByAuthCode(request.Code);
                if (token is null)
                    return Unauthorized(new MastodonApiError("invalid_code"));

                if (token.Active)
                    return BadRequest(new MastodonApiError("Authorization grant is expired or malformed."));
                break;
            
            case "password" when opts.Value.SupportPasswordGrantType:
                if (request.Username is null)
                    return BadRequest(new MastodonApiError("Missing username."));
                
                if (request.Password is null)
                    return BadRequest(new MastodonApiError("Missing password."));
                
                var user = await sessionService.ValidateCredentials(
                    request.Username,
                    request.Password);
                
                if (user is null)
                    return Unauthorized(new MastodonApiError("Processing error: Invalid username or password."));

                token = await managementService.CreateInactiveToken(
                    user,
                    app,
                    app.Scopes);
                
                if (token is null)
                    return BadRequest(new MastodonApiError("General error: Unable to create token."));
                break;
            
            default:
                return BadRequest(new MastodonApiError("Invalid grant type."));
        }

        await managementService.ActivateToken(token);
            
        return Ok(new FetchTokenResponse()
        {
            AccessToken = token.Token,
            Scope = string.Join(" ", token.Scopes!),
            
            CreatedAt = token.CreatedAt.ToUnixTimeSeconds()
        });
    }

    /// <summary>
    /// Revoke an access token to make it no longer valid for use.
    /// </summary>
    /// <param name="request">The parameters for the request.</param>
    /// <returns>Empty on success.</returns>
    [HttpPost]
    [Route("revoke")]
    [Produces("application/json")]
    public async Task<IActionResult> RevokeToken(
        [FromHybrid] RevokeTokenRequest request)
    {
        var app = await repo.FindByClientId(request.ClientId);
        if (app is null || app.ClientSecret != request.ClientSecret)
            return Unauthorized(new MastodonApiError("invalid_client"));
        
        var token = await repo.FindTokenByCode(request.Token);
        if (token is null)
            return Unauthorized(new MastodonApiError("invalid_code"));

        if (token.ParentAppId != app.Id)
            return StatusCode(
                StatusCodes.Status403Forbidden, 
                new MastodonApiError("unauthorized_client"));

        await repo.DeleteToken(token);
        return new JsonResult(new object());
    }
}