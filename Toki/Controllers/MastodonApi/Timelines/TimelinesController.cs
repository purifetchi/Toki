using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;
using Toki.MastodonApi.Schemas.Params;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;
using Toki.Services.Timelines;

namespace Toki.Controllers.MastodonApi.Timelines;

/// <summary>
/// Controller for the "/api/v1/timelines" endpoint.
/// </summary>
[ApiController]
[Route("/api/v1/timelines")]
[EnableCors("MastodonAPI")]
public class TimelinesController(
    StatusRenderer statusRenderer,
    TimelineBuilder timelineBuilder) : ControllerBase 
{
    /// <summary>
    /// Renders the public timeline.
    /// </summary>
    /// <returns>An array of public statuses.</returns>
    [HttpGet]
    [Route("public")]
    [Produces("application/json")]
    [OAuth(manualScopeValidation: true)]
    public async Task<IEnumerable<Status>> PublicTimeline(
        [FromQuery] PaginationParams paginationParams)
    {
        var user = HttpContext.GetOAuthToken()?
            .User;
        // TODO: Give a heck about the query parameters.
        
        var list = await timelineBuilder
            .Filter(post => post.Visibility == PostVisibility.Public)
            .Paginate(paginationParams)
            .GetTimeline();

        return await statusRenderer.RenderManyStatusesForUser(user, list);
    }

    /// <summary>
    /// Renders the home timeline.
    /// </summary>
    /// <returns>An array of statuses.</returns>
    [HttpGet]
    [Route("home")]
    [Produces("application/json")]
    [OAuth("read:statuses")]
    public async Task<IEnumerable<Status>> HomeTimeline(
        [FromQuery] PaginationParams paginationParams)
    {
        // TODO: Give a heck about the query parameters.
        var user = HttpContext.GetOAuthToken()!
            .User;

        var list = await timelineBuilder
            .ViewAs(user)
            .Paginate(paginationParams)
            .GetTimeline();

        return await statusRenderer.RenderManyStatusesForUser(user, list);
    }
}