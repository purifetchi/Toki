using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// The repository for notifications.
/// </summary>
public class NotificationRepository(
    TokiDatabaseContext db)
{
    /// <summary>
    /// Adds a notification.
    /// </summary>
    /// <param name="notification">The notification.</param>
    public async Task Add(Notification notification)
    {
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Gets notifications for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The notifications list.</returns>
    public async Task<IList<Notification>> GetForUser(
        User user)
    {
        var list = await db.Notifications
            .Include(n => n.Actor)
            .Include(n => n.Target)
            .Include(n => n.RelevantPost)
            .Include(n => n.RelevantPost!.Attachments)
            .Where(n => n.TargetId == user.Id)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return list;
    }
}