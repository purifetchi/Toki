using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Users;
using Toki.Extensions;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Objects;
using Toki.MastodonApi.Schemas.Params;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Accounts;

/// <summary>
/// View and manage follow requests.
/// </summary>
[ApiController]
[Route("/api/v1/follow_requests")]
[EnableCors("MastodonAPI")]
public class FollowRequestsController(
    UserRelationService relationService,
    UserRepository repo,
    FollowRepository followRepo,
    AccountRenderer accountRenderer) : ControllerBase
{
    /// <summary>
    /// Returns the pending follow requests.
    /// </summary>
    /// <returns>Array of <see cref="Account"/> on success.</returns>
    [HttpGet]
    [OAuth("read:follows")]
    [Produces("application/json")]
    public async Task<IEnumerable<Account>> GetFollowRequests(
        [FromQuery] PaginationParams paginationParams)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;
        
        var usersView = followRepo.GetFollowRequestsFor(user);
        
        if (paginationParams.MaxId is not null)
            usersView = usersView.Before(paginationParams.MaxId.Value);

        if (paginationParams.SinceId is not null)
            usersView = usersView.After(paginationParams.SinceId.Value);

        var users = await usersView
            .Limit(paginationParams.Limit)
            .Project<User>(fr => fr.From)
            .GetWithMastodonPagination(HttpContext);
                
        return users.Select(
            u => accountRenderer.RenderAccountFrom(u));
    }

    /// <summary>
    /// Authorizes a pending follow request.
    /// </summary>
    /// <param name="id">The id of the account we've authorized.</param>
    /// <returns>A <see cref="Relationship"/> between the requesting user and the accepted user on success.</returns>
    [HttpPost]
    [Route("{id}/authorize")]
    [OAuth("write:follows")]
    [Produces("application/json")]
    public async Task<ActionResult<Relationship>> AuthorizeRelationship(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var other = await repo.FindById(id);
        if (other is null)
            return NotFound(new MastodonApiError("Record not found"));

        var fr = await followRepo.FindFollowRequestByToAndFrom(
            other,
            user);
        
        if (fr is null)
            return NotFound(new MastodonApiError("Record not found"));

        await relationService.AcceptFollowRequest(fr);

        var relationshipInfo = await relationService.GetRelationshipInfoBetween(
            user,
            other);

        return Ok(
            accountRenderer.RenderRelationshipFrom(other, relationshipInfo));
    }
    
    /// <summary>
    /// Rejects a pending follow request.
    /// </summary>
    /// <param name="id">The id of the account we've rejected.</param>
    /// <returns>A <see cref="Relationship"/> between the requesting user and the rejected user on success.</returns>
    [HttpPost]
    [Route("{id}/reject")]
    [OAuth("write:follows")]
    [Produces("application/json")]
    public async Task<ActionResult<Relationship>> RejectRelationship(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var other = await repo.FindById(id);
        if (other is null)
            return NotFound(new MastodonApiError("Record not found"));

        var fr = await followRepo.FindFollowRequestByToAndFrom(
            other,
            user);
        
        if (fr is null)
            return NotFound(new MastodonApiError("Record not found"));

        await relationService.RejectFollowRequest(fr);

        var relationshipInfo = await relationService.GetRelationshipInfoBetween(
            user,
            other);

        return Ok(
            accountRenderer.RenderRelationshipFrom(other, relationshipInfo));
    }
}