using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;
using Toki.ActivityStreams.Objects;

namespace Toki.Controllers;

/// <summary>
/// Controller for the /posts/ route.
/// </summary>
/// <param name="renderer">The post renderer.</param>
[ApiController]
[Route("posts/{id}")]
public class PostsController(
    PostRepository postRepo,
    PostRenderer renderer) : ControllerBase
{
    /// <summary>
    /// Gets a post by its id.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns>The post if it exists, or nothing.</returns>
    [HttpGet]
    [Route("")]
    [Produces("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/json", "application/activity+json")]
    public async Task<ActionResult<ASNote?>> FetchNote([FromRoute] Guid id)
    {
        var post = await postRepo.FindById(id);
        if (post is null)
            return NotFound();

        return renderer.RenderFullNoteFrom(post);
    }
}