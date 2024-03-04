using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
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
}