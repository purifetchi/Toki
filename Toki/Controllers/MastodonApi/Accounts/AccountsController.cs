using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
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
    StatusRenderer statusRenderer,
    UserRepository repo,
    PostManagementService managementService) : ControllerBase
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
    [Route("{id:guid}")]
    [Produces("application/json")]
    public async Task<IActionResult> FetchAccount(
        [FromRoute] Guid id)
    {
        var user = await repo.FindById(id);
        if (user is null)
            return NotFound();

        return Ok(renderer.RenderAccountFrom(user));
    }
    
    /// <summary>
    /// Fetches the data for an account.
    /// </summary>
    /// <param name="id">Its id.</param>
    /// <returns>The <see cref="Account"/> if one exists, an error otherwise.</returns>
    [HttpGet]
    [Route("{id:guid}/statuses")]
    [Produces("application/json")]
    [OAuth(manualScopeValidation: true)]
    public async Task<IActionResult> FetchAccountStatuses(
        [FromRoute] Guid id)
    {
        var us = HttpContext.GetOAuthToken()?
            .User;
        
        var user = await repo.FindById(id);
        if (user is null)
            return NotFound();

        var posts = await managementService.GetPostsForUser(
            user,
            us);

        return Ok(
            posts.Select(statusRenderer.RenderForPost));
    }
}