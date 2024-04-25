using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.Extensions;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;
using Toki.MastodonApi.Schemas.Params;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Accounts;

/// <summary>
/// View your bookmarks.
/// </summary>
[ApiController]
[Route("/api/v1/bookmarks")]
[EnableCors("MastodonAPI")]
public class BookmarksController(
    PostRepository postRepo,
    StatusRenderer statusRenderer) : ControllerBase
{
    /// <summary>
    /// Statuses the user has bookmarked.
    /// </summary>
    /// <returns>A list of <see cref="Status"/> on success.</returns>
    [HttpGet]
    [OAuth("read:bookmarks")]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<Status>>> GetBookmarks(
        [FromQuery] PaginationParams paginationParams)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        // TODO: We really need to unify adding post rendering data.
        var bookmarksView = postRepo.GetBookmarksForUser(user);
        var bookmarks = await bookmarksView
            .WithMastodonParams(paginationParams)
            .Project<Post>(b => b.Post)
            .GetWithMastodonPagination(HttpContext);

        return Ok(
            await statusRenderer.RenderManyStatusesForUser(user, bookmarks));
    }
}