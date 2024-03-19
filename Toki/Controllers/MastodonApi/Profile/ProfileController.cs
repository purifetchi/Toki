using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Profile;

/// <summary>
/// Controller for the "/api/v1/profile" route.
/// </summary>
/// <param name="repo">The user repository.</param>
/// <param name="accountRenderer">The mastodon account renderer.</param>
[ApiController]
[Route("/api/v1/profile")]
[EnableCors("MastodonAPI")]
public class ProfileController(
    UserRepository repo,
    AccountRenderer accountRenderer) : ControllerBase
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

        user.AvatarUrl = null;
        await repo.Update(user);

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

        user.BannerUrl = null;
        await repo.Update(user);

        return accountRenderer.RenderAccountFrom(user, true);
    }
}