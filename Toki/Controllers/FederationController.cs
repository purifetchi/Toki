using Microsoft.AspNetCore.Mvc;
using Toki.ActivityStreams.Objects;

namespace Toki.Controllers;

/// <summary>
/// The base federation controller.
/// </summary>
[ApiController]
[Route("/")]
public class FederationController : ControllerBase
{
    /// <summary>
    /// The inbox.
    /// </summary>
    /// <returns>The result.</returns>
    [Route("inbox")]
    [Consumes("application/activity+json")]
    [HttpPost]
    public async Task<IActionResult> Inbox(
        [FromBody] ASObject? activity)
    {
        Console.WriteLine($"[INBOX] Received new activity of type {activity?.Type}");
        return Ok();
    }
}