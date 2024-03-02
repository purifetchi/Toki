using Microsoft.AspNetCore.Mvc;
using Toki.MastodonApi.Schemas.Requests.Apps;

namespace Toki.Controllers.MastodonApi.Apps;

/// <summary>
/// The controller for the "/api/v1/apps" route in Mastodon.
/// </summary>
[ApiController]
[Route("/api/v1/apps")]
public class AppsController : ControllerBase
{
    /// <summary>
    /// Sent by the client when they want to register a new oauth2 app.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>Either an application, or an error.</returns>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateApp(
        [FromForm] CreateApplicationRequest request)
    {
        return Ok();
    }
}