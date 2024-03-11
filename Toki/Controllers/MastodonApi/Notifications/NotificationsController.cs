using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.MastodonApi.Schemas.Objects;
using Toki.Middleware.OAuth2;

namespace Toki.Controllers.MastodonApi.Notifications;

/// <summary>
/// The controller for the "/api/v1/notifications" endpoint
/// </summary>
[ApiController]
[EnableCors("MastodonAPI")]
[Route("/api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    /// <summary>
    /// Gets the notifications related to the users.
    /// </summary>
    /// <returns>A list of the notifications, or an error.</returns>
    [HttpGet]
    [OAuth("read:notifications")]
    public async Task<IEnumerable<Notification>> GetNotifications()
    {
        // TODO: Query parameters as described by: https://docs.joinmastodon.org/methods/notifications/
        // TODO: Stub

        return Array.Empty<Notification>();
    }
}