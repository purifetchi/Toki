using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.Extensions;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;
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
    public async Task<ActionResult<IEnumerable<Status>>> GetBookmarks()
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        // TODO: We really need to unify adding post rendering data.
        // TODO: Pagination.
        var query = await postRepo.CreateCustomBookmarkedPostQuery()
            .Where(b => b.UserId == user.Id)
            .Include(b => b.Post)
            .Include(b => b.Post.Author)
            .Include(b => b.Post.Attachments)
            .Include(b => b.Post.Parent)
            .Include(b => b.Post.Parent!.Author)
            .OrderByDescending(b => b.Id)
            .Select(b => b.Post)
            .ToListAsync();

        return Ok(
            await statusRenderer.RenderManyStatusesForUser(user, query));
    }
}