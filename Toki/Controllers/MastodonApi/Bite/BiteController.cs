using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Bites;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.MastodonApi.Schemas.Errors;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Bite;

/// <summary>
/// The "/api/v1/bite" handler.
/// </summary>
[ApiController]
[Route("/api/v1/bite")]
[EnableCors("MastodonAPI")]
public class BiteController(
    UserRepository userRepo,
    BiteService biteService) : ControllerBase
{
    /// <summary>
    /// Bites the object specified by `id`.
    /// </summary>
    /// <param name="id">The object.</param>
    /// <returns>An empty JSON object on success.</returns>
    [HttpPost]
    [OAuth("write:bites")]
    [Route("")]
    public async Task<IActionResult> Bite(
        [FromQuery] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var targetUser = await userRepo.FindById(id);
        if (targetUser is null)
            return UnprocessableEntity(new MastodonApiError("Validation error: Toki only supports biting users."));

        await biteService.Bite(
            user,
            targetUser);
        
        return new JsonResult(new object());
    }
}