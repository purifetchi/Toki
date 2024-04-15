using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Models.DTO;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Users;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Profile;

/// <summary>
/// Controller for the "/api/v1/profile" route.
/// </summary>
/// <param name="accountRenderer">The mastodon account renderer.</param>
/// <param name="managementService">The user management service.</param>
[ApiController]
[Route("/api/v1/profile")]
[EnableCors("MastodonAPI")]
public class ProfileController(
    AccountRenderer accountRenderer,
    UserManagementService managementService) : ControllerBase
{
    /// <summary>
    /// Deletes the avatar associated with the user’s profile.
    /// </summary>
    /// <returns><see cref="Account"/> on success.</returns>
    [HttpDelete]
    [Route("avatar")]
    [OAuth("write:accounts")]
    [Produces("application/json")]
    public async Task<Account> DeleteAvatar()
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var updateRequest = new UserUpdateRequest
        {
            RemoveAvatar = true
        };
        
        await managementService.Update(
            user,
            updateRequest);

        return accountRenderer.RenderAccountFrom(user, true);
    }
    
    /// <summary>
    /// Deletes the header image associated with the user’s profile.
    /// </summary>
    /// <returns><see cref="Account"/> on success.</returns>
    [HttpDelete]
    [Route("header")]
    [OAuth("write:accounts")]
    [Produces("application/json")]
    public async Task<Account> DeleteHeader()
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var updateRequest = new UserUpdateRequest
        {
            RemoveHeader = true
        };
        
        await managementService.Update(
            user,
            updateRequest);

        return accountRenderer.RenderAccountFrom(user, true);
    }
}