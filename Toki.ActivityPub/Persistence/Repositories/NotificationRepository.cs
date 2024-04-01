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
    /// Gets a notification by its ID.
    /// </summary>
    /// <param name="id">The ID of the notification.</param>
    /// <returns>The notification, if one exists.</returns>
    public async Task<Notification?> FindById(Ulid id)
    {
        return await db.Notifications
            .Include(n => n.Actor)
            .Include(n => n.Target)
            .Include(n => n.RelevantPost)
            .Include(n => n.RelevantPost!.Attachments)
            .FirstOrDefaultAsync(n => n.Id == id);
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

    /// <summary>
    /// Deletes all the notifications for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    public async Task DeleteAllForUser(User user)
    {
        await db.Notifications
            .Where(n => n.Target == user)
            .ExecuteDeleteAsync();
    }
    
    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="notif">The notification.</param>
    public async Task Delete(Notification notif)
    {
        db.Notifications.Remove(notif);
        await db.SaveChangesAsync();
    }
}