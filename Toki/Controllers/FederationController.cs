using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Cryptography;
using Toki.ActivityPub.Jobs.Federation;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityStreams.Objects;
using Toki.Extensions;

namespace Toki.Controllers;

/// <summary>
/// The base federation controller.
/// </summary>
[ApiController]
[Route("/")]
public class FederationController(
    ActivityPubMessageValidationService validator,
    InstanceActorResolver instanceActorResolver)
    : ControllerBase
{
    /// <summary>
    /// The inbox.
    /// </summary>
    /// <returns>The result.</returns>
    [Route("inbox")]
    [Consumes("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/activity+json")]
    [HttpPost]
    public async Task<IActionResult> Inbox(
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

    /// <summary>
    /// Returns the instance actor for this instance. Used for signed fetches.
    /// </summary>
    [Route("actor")]
    [Produces("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/activity+json")]
    [HttpGet]
    public async Task<ActionResult<ASActor>> InstanceActor()
    {
        return await instanceActorResolver.RenderInstanceActor();
    }
}