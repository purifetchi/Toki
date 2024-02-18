using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityStreams.Objects;

namespace Toki.Controllers;

/// <summary>
/// The users controller for ActivityPub interoperability.
/// </summary>
[ApiController]
[Route("users/{:handle}")]
public class UsersController(UserRepository repo)
    : ControllerBase
{
    /// <summary>
    /// Gets a user's actor by their handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>The user if they exist, or nothing.</returns>
    [HttpGet]
    [Route("")]
    [Produces("application/json", "application/activity+json")]
    public async Task<ActionResult<ASActor?>> FetchActor([FromRoute] string handle)
    {
        var user = await repo.FindByHandle(handle);
        if (user is null)
            return NotFound();

        return user.ToASActor();
    }

    /// <summary>
    /// The user inbox.
    /// </summary>
    /// <param name="handle">The receiving user's handle.</param>
    /// <param name="activity">The activity.</param>
    /// <returns>The result.</returns>
    [HttpPost]
    [Consumes("application/activity+json")]
    [Route("inbox")]
    public async Task<IActionResult> Inbox(
        [FromRoute] string handle,
        [FromBody] ASObject? activity)
    {
        // TODO: This will just go to the inbox handler.
        return Ok();
    }
}