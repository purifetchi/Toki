using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;

namespace Toki.Controllers.MastodonApi.Timelines;

/// <summary>
/// Controller for the "/api/v1/timelines" endpoint.
/// </summary>
[ApiController]
[Route("/api/v1/timelines")]
[EnableCors("MastodonAPI")]
public class TimelinesController(
    StatusRenderer statusRenderer,
    PostRepository repo) : ControllerBase 
{
    /// <summary>
    /// Renders the public timeline.
    /// </summary>
    /// <returns>An array of public statuses.</returns>
    [HttpGet]
    [Route("public")]
    [Produces("application/json")]
    public async Task<IEnumerable<Status>> PublicTimeline()
    {
        // TODO: Give a heck about the query parameters.
        
        var list = await repo.CreateCustomQuery()
            .Include(post => post.Author)
            .Include(post => post.Parent)
            .OrderByDescending(post => post.ReceivedAt)
            .Where(post => post.Visibility == PostVisibility.Public)
            .ToListAsync();

        return list.Select(statusRenderer.RenderForPost);
    }
}