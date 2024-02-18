using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.WebFinger;

namespace Toki.Controllers;

/// <summary>
/// The controller for the .well-known route.
/// </summary>
[ApiController]
[Route(".well-known")]
public class WellKnownController : ControllerBase
{
    /// <summary>
    /// Runs a webfinger query.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <returns>Either a webfinger response, or nothing.</returns>
    [Route("webfinger")]
    [HttpGet]
    public async Task<ActionResult<WebFingerResponse?>> WebFinger([FromQuery] string resource)
    {
        return NotFound();
    }
}