using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.Extensions;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Objects;
using Toki.MastodonApi.Schemas.Params;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Notifications;

/// <summary>
/// The controller for the "/api/v1/notifications" endpoint
/// </summary>
[ApiController]
[EnableCors("MastodonAPI")]
[Route("/api/v1/notifications")]
public class NotificationsController(
    NotificationRepository repo,
    AccountRenderer accountRenderer,
    StatusRenderer statusRenderer) : ControllerBase
{
    /// <summary>
    /// Gets the notifications related to the users.
    /// </summary>
    /// <returns>A list of the notifications, or an error.</returns>
    [HttpGet]
    [OAuth("read:notifications")]
    [Produces("application/json")]
    public async Task<IEnumerable<Notification>> GetNotifications(
        [FromQuery] PaginationParams paginationParams)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;
        
        // TODO: Query parameters as described by: https://docs.joinmastodon.org/methods/notifications/
        var notifsView = repo.GetForUser(user);
        var notifs = await notifsView
            .WithMastodonParams(paginationParams)
            .GetWithMastodonPagination(HttpContext);
        
        return notifs.Select(n => new Notification
        {
            Id = n.Id.ToString(),
            Type = n.Type.ToMastodonNotificationType(),
            CreatedAt = n.CreatedAt,
            
            Account = accountRenderer.RenderAccountFrom(n.Actor),
            Status = n.RelevantPost is not null ?
                statusRenderer.RenderForPost(n.RelevantPost) :
                null
        });
    }

    /// <summary>
    /// View information about a notification with a given ID.
    /// </summary>
    /// <param name="id">The ID of the Notification in the database.</param>
    /// <returns>A <see cref="Notification"/> on success.</returns>
    [HttpGet]
    [OAuth("read:notifications")]
    [Produces("application/json")]
    [Route("{id}")]
    public async Task<ActionResult<Notification>> GetNotification(
        [FromRoute] Ulid id)
    {
        var notif = await repo.FindById(id);
        if (notif is null)
            return NotFound(new MastodonApiError("Record not found."));

        return Ok(new Notification
        {
            Id = notif.Id.ToString(),
            Type = notif.Type.ToMastodonNotificationType(),
            CreatedAt = notif.CreatedAt,
            
            Account = accountRenderer.RenderAccountFrom(notif.Actor),
            Status = notif.RelevantPost is not null ?
                statusRenderer.RenderForPost(notif.RelevantPost) :
                null
        });
    }

    /// <summary>
    /// Clear all notifications from the server.
    /// </summary>
    /// <returns>Empty.</returns>
    [HttpPost]
    [OAuth("write:notifications")]
    [Produces("application/json")]
    [Route("clear")]
    public async Task<IActionResult> ClearNotifications()
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        await repo.DeleteAllForUser(user);
        return new JsonResult(new object());
    }
    
    /// <summary>
    /// Dismiss a single notification from the server.
    /// </summary>
    /// <param name="id">The ID of the Notification in the database.</param>
    /// <returns>Empty.</returns>
    [HttpPost]
    [OAuth("write:notifications")]
    [Produces("application/json")]
    [Route("{id}/dismiss")]
    public async Task<IActionResult> DismissNotification(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var notif = await repo.FindById(id);
        if (notif is null)
            return NotFound(new MastodonApiError("Record not found."));
        
        await repo.Delete(notif);
        return new JsonResult(new object());
    }
}