using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Cryptography;
using Toki.ActivityPub.Jobs.Federation;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;
using Toki.ActivityStreams.Objects;
using Toki.Extensions;

namespace Toki.Controllers;

/// <summary>
/// The users controller for ActivityPub interoperability.
/// </summary>
[ApiController]
[Route("users/{handle}")]
public class UsersController(
    UserRepository repo,
    UserRenderer renderer,
    ActivityPubMessageValidationService validator)
    : ControllerBase
{
    /// <summary>
    /// Gets a user's actor by their handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>The user if they exist, or nothing.</returns>
    [HttpGet]
    [Route("")]
    [Produces("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/json", "application/activity+json")]
    public async Task<ActionResult<ASActor?>> FetchActor([FromRoute] string handle)
    {
        var user = await repo.FindByHandle(handle);
        if (user is null)
            return NotFound();

        return await renderer.RenderFullActorFrom(user);
    }

    /// <summary>
    /// The user inbox.
    /// </summary>
    /// <param name="handle">The receiving user's handle.</param>
    /// <param name="asObject">The activity.</param>
    /// <returns>The result.</returns>
    [HttpPost]
    [Consumes("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/activity+json")]
    [Route("inbox")]
    public async Task<IActionResult> Inbox(
        [FromRoute] string handle,
        [FromBody] ASObject? asObject)
    {
        if (!await validator.Validate(HttpContext.Request.ToTokiHttpRequest(), asObject))
            return Unauthorized();
        
        // TODO: This is really ugly.
        var data = JsonSerializer.Serialize(asObject);
        BackgroundJob.Enqueue<InboxHandlerJob>(job =>
            job.HandleActivity(data));
        return Ok();
    }
}