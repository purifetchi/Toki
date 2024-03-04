using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Accounts;

/// <summary>
/// The "/api/v1/accounts" handler.
/// </summary>
[ApiController]
[Route("/api/v1/accounts")]
[EnableCors("MastodonAPI")]
public class AccountsController(
    AccountRenderer renderer,
    UserRepository repo) : ControllerBase
{
    /// <summary>
    /// Verifies credentials for an app.
    /// </summary>
    /// <returns>Either unauthorized, or the app.</returns>
    [HttpGet]
    [Route("verify_credentials")]
    [Produces("application/json")]
    [OAuth("read:accounts")]
    public IActionResult VerifyCredentials()
    {
        var token = HttpContext.GetOAuthToken()!;
        
        return Ok(
            renderer.RenderAccountFrom(
                token.User, 
                renderCredentialAccount: true)
            );
    }

    /// <summary>
    /// Fetches the data for an account.
    /// </summary>
    /// <param name="id">Its id.</param>
    /// <returns>The <see cref="Account"/> if one exists, an error otherwise.</returns>
    [HttpGet]
    [Route("{id}")]
    [Produces("application/json")]
    public async Task<IActionResult> FetchAccount(
        [FromRoute] Guid id)
    {
        var user = await repo.FindById(id);
        if (user is null)
            return NotFound();

        return Ok(renderer.RenderAccountFrom(user));
    }
}