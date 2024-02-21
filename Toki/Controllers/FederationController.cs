using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    HttpSignatureValidator signatureValidator,
    TokiDatabaseContext db)
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
        if (asObject is not ASActivity activity)
            return BadRequest();
        
        var signature = Signature.FromHttpRequest(
            HttpContext.Request.ToTokiHttpRequest());
        
        if (signature is null)
            return Unauthorized();

        var key = await db.Keypairs
            .Where(k => k.RemoteId == signature.KeyId)
            .FirstOrDefaultAsync();

        // TODO: Fetch the actor in this case.
        if (key is null)
            return BadRequest();
        
        if (!signatureValidator.Validate(signature, key.PublicKey))
            return Unauthorized();
        
        Console.WriteLine($"[INBOX] Received new activity of type {activity?.Type}");
        return Ok();
    }
}