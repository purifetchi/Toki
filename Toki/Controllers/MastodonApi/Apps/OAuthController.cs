using Microsoft.AspNetCore.Mvc;
using Toki.MastodonApi.Schemas.Requests.Apps;

namespace Toki.Controllers.MastodonApi.Apps;

/// <summary>
/// The "/oauth" controller for Mastodon api.
/// </summary>
[ApiController]
[Route("/oauth")]
public class OAuthController : ControllerBase
{
    /// <summary>
    /// Authorizes a token.
    /// </summary>
    /// <param name="request">The authorization request.</param>
    /// <returns>Either a redirect on success, or an error.</returns>
    [HttpGet]
    [Route("authorize")]
    public async Task<IActionResult> Authorize(
        [FromQuery] OAuthAuthorizeRequest request)
    {
        return Ok();
    }
}