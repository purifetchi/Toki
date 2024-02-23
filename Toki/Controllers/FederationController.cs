using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Cryptography;
using Toki.ActivityPub.Jobs.Federation;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Objects;
using Toki.Extensions;
using Toki.HTTPSignatures;
using Toki.HTTPSignatures.Models;

namespace Toki.Controllers;

/// <summary>
/// The base federation controller.
/// </summary>
[ApiController]
[Route("/")]
public class FederationController(
    ActivityPubMessageValidationService validator)
    : ControllerBase
{
    /// <summary>
    /// The inbox.
    /// </summary>
    /// <returns>The result.</returns>
    [Route("inbox")]
    [Consumes("application/activity+json")]
    [HttpPost]
    public async Task<IActionResult> Inbox(
        [FromBody] ASObject? asObject)
    {
        if (!await validator.Validate(HttpContext.Request.ToTokiHttpRequest(), asObject))
            return Unauthorized();
        
        BackgroundJob.Enqueue<InboxHandlerJob>(job =>
            job.HandleActivity(asObject!));
        return Ok();
    }
}