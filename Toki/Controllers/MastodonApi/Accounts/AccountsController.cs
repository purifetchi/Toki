using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Users;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Objects;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;
using Toki.Services.Timelines;

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
    FollowRepository followRepo,
    TimelineBuilder timelineBuilder) : ControllerBase
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
    /// <param name="pinned">Should we fetch only pinned posts?</param>
    /// <returns>The <see cref="Account"/> if one exists, an error otherwise.</returns>
    [HttpGet]
    [Route("{id:guid}/statuses")]
    [Produces("application/json")]
    [OAuth(manualScopeValidation: true)]
    public async Task<IActionResult> FetchAccountStatuses(
        [FromRoute] Guid id,
        [FromQuery] bool pinned = false)
    {
        // TODO: Support pinned posts.
        if (pinned)
            return Ok(Array.Empty<Status>());
        
        var us = HttpContext.GetOAuthToken()?
            .User;
        
        var user = await repo.FindById(id);
        if (user is null)
            return NotFound();

        var posts = await timelineBuilder
            .ViewAs(us)
            .Filter(post => post.AuthorId == id)
            .GetTimeline();

        return Ok(
            posts.Select(statusRenderer.RenderForPost));
    }

    /// <summary>
    /// Accounts which follow the given account, if network is not hidden by the account owner.
    /// </summary>
    /// <param name="id">The ID of the Account in the database.</param>
    /// <returns>The list of <see cref="Account"/></returns>
    [HttpGet]
    [Route("{id:guid}/followers")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAccountFollowers(
        [FromRoute] Guid id)
    {
        var user = await repo.FindById(id);
        if (user is null)
            return NotFound(new MastodonApiError("Record not found."));
        
        // TODO: Implement pagination and limits.
        var followers = await followRepo.GetFollowersFor(user);
        return Ok(followers
            .Select(u => renderer.RenderAccountFrom(u)));
    }
    
    /// <summary>
    /// Accounts which the given account is following, if network is not hidden by the account owner.
    /// </summary>
    /// <param name="id">The ID of the Account in the database.</param>
    /// <returns>The list of <see cref="Account"/></returns>
    [HttpGet]
    [Route("{id:guid}/following")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAccountFollowing(
        [FromRoute] Guid id)
    {
        var user = await repo.FindById(id);
        if (user is null)
            return NotFound(new MastodonApiError("Record not found."));
        
        // TODO: Implement pagination and limits.
        var followers = await followRepo.GetFollowingFor(user);
        return Ok(followers
            .Select(u => renderer.RenderAccountFrom(u)));
    }
}