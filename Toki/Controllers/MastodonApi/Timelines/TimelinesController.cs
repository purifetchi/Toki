using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

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
            .Include(post => post.Attachments)
            .Include(post => post.Boosting)
            .ThenInclude(boost => boost!.Author)
            .OrderByDescending(post => post.ReceivedAt)
            .Where(post => post.Visibility == PostVisibility.Public)
            .ToListAsync();

        return list.Select(statusRenderer.RenderForPost);
    }

    /// <summary>
    /// Renders the home timeline.
    /// </summary>
    /// <returns>An array of statuses.</returns>
    [HttpGet]
    [Route("home")]
    [Produces("application/json")]
    [OAuth("read:statuses")]
    public async Task<IEnumerable<Status>> HomeTimeline()
    {
        // TODO: Give a heck about the query parameters.
        var user = HttpContext.GetOAuthToken()!
            .User;

        // TODO: PLEASE make this look nicer.
        var list = await repo.CreateCustomQuery()
            .Include(post => post.Author)
            .ThenInclude(author => author!.FollowerRelations)
            .Include(post => post.Parent)
            .Include(post => post.Attachments)
            .Include(post => post.Boosting)
            .ThenInclude(boost => boost!.Author)
            .OrderByDescending(post => post.ReceivedAt)
            .Where(post => post.AuthorId == user.Id ||
                           (post.Author.FollowerRelations != null &&
                            post.Author.FollowerRelations.Any(fr => fr.FollowerId == user.Id)))
            .Where(post => post.Visibility != PostVisibility.Direct)
            .ToListAsync();// TODO: Check whether the user is a target of a direct message.

        return list.Select(statusRenderer.RenderForPost);
    }
}