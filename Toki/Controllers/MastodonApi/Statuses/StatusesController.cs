using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.Binding;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Requests.Statuses;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Statuses;

/// <summary>
/// The controller for the "/api/v1/statuses" endpoint.
/// </summary>
[ApiController]
[Route("/api/v1/statuses")]
[EnableCors("MastodonAPI")]
public class StatusesController(
    PostManagementService postManagementService,
    PostRepository repo,
    StatusRenderer statusRenderer) : ControllerBase
{
    /// <summary>
    /// Posts a request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Consumes("application/json", "application/x-www-form-urlencoded")]
    [Produces("application/json")]
    [OAuth("write:statuses")]
    public async Task<IActionResult> PostStatus(
        [FromHybrid] PostStatusRequest request)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        if (request.Status is null)
            return BadRequest(new MastodonApiError("Validation error: Post cannot be empty."));

        var post = await postManagementService.Create(
            user,
            request.Status,
            request.GetVisibility());
        
        if (post is null)
            return BadRequest(new MastodonApiError("Posting error: Cannot post status."));

        return Ok(
            statusRenderer.RenderForPost(post));
    }

    /// <summary>
    /// Fetches a status.
    /// </summary>
    /// <param name="id">The id of the status.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpGet]
    [Route("{id:guid}")]
    [OAuth(manualScopeValidation: true)]
    [Produces("application/json")]
    public async Task<IActionResult> FetchStatus(
        [FromRoute] Guid id)
    {
        var user = HttpContext.GetOAuthToken()?
            .User;
        
        var post = await repo.FindById(id);
        if (post is null || !post.VisibleByUser(user))
            return NotFound(new MastodonApiError("Record not found."));

        return Ok(
            statusRenderer.RenderForPost(post));
    }

    /// <summary>
    /// Favourites a post.
    /// </summary>
    /// <param name="id">The id of the post.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Produces("application/json")]
    [OAuth("write:favourites")]
    [Route("{id:guid}/favourite")]
    public async Task<IActionResult> Favourite(
        [FromRoute] Guid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var post = await repo.FindById(id);
        if (post is null || !post.VisibleByUser(user))
            return NotFound(new MastodonApiError("Record not found."));

        await postManagementService.Like(
            user,
            post);
        
        return Ok(
            statusRenderer.RenderForPost(post));
    }
}