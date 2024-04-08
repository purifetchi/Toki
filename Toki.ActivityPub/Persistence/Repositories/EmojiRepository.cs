using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// The emoji repository.
/// </summary>
/// <param name="db">The database.</param>
public class EmojiRepository(
    TokiDatabaseContext db)
{
    /// <summary>
    /// Adds an emoji to the database.
    /// </summary>
    /// <param name="emoji">The emoji.</param>
    public async Task Add(Emoji emoji)
    {
        db.Emojis.Add(emoji);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Adds a range of emojis to the database.
    /// </summary>
    /// <param name="emojis">The emojis.</param>
    public async Task AddMany(IEnumerable<Emoji> emojis)
    {
        db.Emojis.AddRange(emojis);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Finds many emojis by their ids.
    /// </summary>
    /// <param name="ids">The ids.</param>
    /// <returns>The ids.</returns>
    public async Task<IReadOnlyList<Emoji>> FindManyByIds(
        IReadOnlyList<Ulid> ids)
    {
        return await db.Emojis
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();
    }
    
    /// <summary>
    /// Finds many emojis by their name and instance.
    /// </summary>
    /// <param name="names">The names.</param>
    /// <param name="instance">The instance.</param>
    /// <returns>The ids.</returns>
    public async Task<IReadOnlyList<Emoji>> FindManyByNameAndInstance(
        IReadOnlyList<string> names,
        RemoteInstance? instance)
    {
        return await db.Emojis
            .Where(e => names.Contains(e.Shortcode))
            .Where(e => e.ParentInstance == instance)
            .ToListAsync();
    }
    
    /// <summary>
    /// Finds many emojis by their  instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>The ids.</returns>
    public async Task<IReadOnlyList<Emoji>> FindManyByInstance(
        RemoteInstance? instance)
    {
        return await db.Emojis
            .Where(e => e.ParentInstance == instance)
            .ToListAsync();
    }
}