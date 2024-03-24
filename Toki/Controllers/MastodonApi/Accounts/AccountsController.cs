using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Users;
using Toki.Binding;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Objects;
using Toki.MastodonApi.Schemas.Params;
using Toki.MastodonApi.Schemas.Requests.Accounts;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;
using Toki.Services.Drive;
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
    UserRelationService relationService,
    TimelineBuilder timelineBuilder,
    DriveService drive,
    UserManagementService managementService) : ControllerBase
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
        [FromRoute] Ulid id)
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
    /// <param name="filters">The filters for statuses.</param>
    /// <param name="paginationParams">The pagination params.</param>
    /// <returns>The <see cref="Account"/> if one exists, an error otherwise.</returns>
    [HttpGet]
    [Route("{id}/statuses")]
    [Produces("application/json")]
    [OAuth(manualScopeValidation: true)]
    public async Task<IActionResult> FetchAccountStatuses(
        [FromRoute] Ulid id,
        [FromQuery] StatusesFilters filters,
        [FromQuery] PaginationParams paginationParams)
    {
        // TODO: Support pinned posts.
        if (filters.Pinned)
            return Ok(Array.Empty<Status>());
        
        var us = HttpContext.GetOAuthToken()?
            .User;
        
        var user = await repo.FindById(id);
        if (user is null)
            return NotFound();

        var query = timelineBuilder
            .ViewAs(us)
            .Paginate(paginationParams)
            .Filter(post => post.AuthorId == id);

        if (filters.OnlyMedia)
            query = query.Filter(post => post.Attachments != null && post.Attachments.Count > 0);

        if (filters.ExcludeBoosts)
            query = query.Filter(post => post.BoostingId == null);

        if (filters.ExcludeReplies)
            query = query.Filter(post => post.ParentId == null);

        var posts = await query.GetTimeline();
        return Ok(await statusRenderer.RenderManyStatusesForUser(us, posts));
    }

    /// <summary>
    /// Accounts which follow the given account, if network is not hidden by the account owner.
    /// </summary>
    /// <param name="id">The ID of the Account in the database.</param>
    /// <returns>The list of <see cref="Account"/></returns>
    [HttpGet]
    [Route("{id}/followers")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAccountFollowers(
        [FromRoute] Ulid id)
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
    [Route("{id}/following")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAccountFollowing(
        [FromRoute] Ulid id)
    {
        var user = await repo.FindById(id);
        if (user is null)
            return NotFound(new MastodonApiError("Record not found."));
        
        // TODO: Implement pagination and limits.
        var followers = await followRepo.GetFollowingFor(user);
        return Ok(followers
            .Select(u => renderer.RenderAccountFrom(u)));
    }

    /// <summary>
    /// Gets all of the relationships between this user and users specified by "id".
    /// </summary>
    /// <param name="mastoId">The ids as passed in by any mastodon-fe client.</param>
    /// <param name="normalId">The ids as passed in by any normal client.</param>
    /// <returns>An array of Relationship.</returns>
    [HttpGet]
    [Route("relationships")]
    [Produces("application/json")]
    [OAuth("read:follows")]
    public async Task<IActionResult> GetRelationships(
        [FromQuery(Name = "id[]")] Ulid[] mastoId,
        [FromQuery(Name = "id")] Ulid[] normalId)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var id = normalId.Length > 0 ? normalId : mastoId;
        var results = new List<Relationship>();
        foreach (var uid in id)
        {
            var target = await repo.FindById(uid);
            if (target is null)
                continue;

            var relationshipInfo = await relationService.GetRelationshipInfoBetween(
                user,
                target);
            
            results.Add(
                renderer.RenderRelationshipFrom(target, relationshipInfo));
        }
        
        return Ok(results);
    }

    /// <summary>
    /// Follow the given account. Can also be used to update whether to show reblogs or enable notifications.
    /// </summary>
    /// <param name="id">The id of the account.</param>
    /// <returns>Either a <see cref="Relationship"/> or an error.</returns>
    [HttpPost]
    [Route("{id}/follow")]
    [OAuth("write:follows")]
    public async Task<ActionResult<Relationship>> FollowAccount(
        [FromRoute] Ulid id)
    {
        // TODO: Support reblogs and notify.
        var user = HttpContext.GetOAuthToken()!
            .User;
        
        var them = await repo.FindById(id);
        if (them is null)
            return NotFound(new MastodonApiError("Record not found."));

        if (user.Id == them.Id)
            return Forbid();
        
        await relationService.RequestFollow(
            user,
            them);

        var relationship = await relationService.GetRelationshipInfoBetween(
            user,
            them);

        return Ok(
            renderer.RenderRelationshipFrom(them, relationship));
    }

    /// <summary>
    /// Quickly lookup a username to see if it is available, skipping WebFinger resolution.
    /// </summary>
    /// <param name="acct">The username or WebFinger address to lookup.</param>
    /// <returns>Either an <see cref="Account"/> or an error.</returns>
    [HttpGet]
    [Route("lookup")]
    public async Task<ActionResult<Account>> Lookup(
        [FromQuery] string? acct)
    {
        if (acct is null)
            return UnprocessableEntity(new MastodonApiError("Missing acct."));

        var user = await repo.FindByHandle(acct);
        if (user is null)
            return NotFound(new MastodonApiError("Record not found"));

        return renderer.RenderAccountFrom(user);
    }

    /// <summary>
    /// Update the user’s display and preferences.
    /// </summary>
    /// <param name="request">The new settings.</param>
    /// <returns>On success, the user’s own <see cref="Account"/> with source attribute. Error otherwise.</returns>
    [HttpPatch]
    [Route("update_credentials")]
    [OAuth("write:accounts")]
    public async Task<IActionResult> UpdateCredentials(
        [FromHybrid] UpdateCredentialsRequest request)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        user.DisplayName = request.DisplayName ?? user.DisplayName;
        user.Bio = request.Bio ?? user.Bio;
        user.RequiresFollowApproval = request.RequiresFollowApproval ?? user.RequiresFollowApproval;

        if (request.Avatar is not null)
        {
            var url = await drive.Store(request.Avatar);
            user.AvatarUrl = url;
        }
        
        if (request.Header is not null)
        {
            var url = await drive.Store(request.Header);
            user.BannerUrl = url;
        }
        
        await managementService.Update(user);
        
        return Ok(
            renderer.RenderAccountFrom(user));
    }
}