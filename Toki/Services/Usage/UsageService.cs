using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityPub.Resolvers;
using Toki.Extensions;
using Toki.Services.Usage.Models;

namespace Toki.Services.Usage;

/// <summary>
/// The service responsible for fetching usage statistics.
/// </summary>
/// <param name="db">The database.</param>
/// <param name="cache">The cache.</param>
public class UsageService(
    TokiDatabaseContext db,
    IDistributedCache cache)
{
    /// <summary>
    /// The cache key for the statistics.
    /// </summary>
    private const string CACHE_KEY = "toki:instance:stats";

    /// <summary>
    /// The amount of days for the cache to expire.
    /// </summary>
    private const int CACHE_EXPIRATION_IN_DAYS = 1;
    
    /// <summary>
    /// Gets the statistics for this instance.
    /// </summary>
    /// <returns>The usage statistics.</returns>
    public async Task<UsageStatistics> GetStatistics()
    {
        var cachedStats = await cache.GetAsync<UsageStatistics>(CACHE_KEY);
        if (cachedStats is not null)
            return cachedStats;

        var stats = await ComputeStatistics();
        await cache.SetAsync(CACHE_KEY, stats, new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(CACHE_EXPIRATION_IN_DAYS)
        });

        return stats;
    }

    /// <summary>
    /// Computes the statistics for this instance.
    /// </summary>
    /// <returns>The statistics.</returns>
    private async Task<UsageStatistics> ComputeStatistics()
    {
        var lastMonth = DateTimeOffset.UtcNow
            .AddMonths(-1);

        var monthUsers = await db.Posts
            .Where(p => p.CreatedAt > lastMonth)
            .Include(p => p.Author)
            .Where(p => !p.Author.IsRemote)
            .Select(p => p.AuthorId)
            .Distinct()
            .CountAsync();
        
        var halfYear = DateTimeOffset.UtcNow
            .AddMonths(-6);

        var halfYearUsers = await db.Posts
            .Where(p => p.CreatedAt > halfYear)
            .Include(p => p.Author)
            .Where(p => !p.Author.IsRemote)
            .Select(p => p.AuthorId)
            .Distinct()
            .CountAsync();

        var localPostCount = await db.Posts
            .Include(p => p.Author)
            .Where(p => !p.Author.IsRemote)
            .CountAsync();

        var userCount = await db.Users
            .Where(u => !u.IsRemote)
            .Where(u => u.Handle != InstanceActorResolver.INSTANCE_ACTOR_NAME)
            .CountAsync();

        var peerCount = await db.Instances
            .CountAsync();

        return new UsageStatistics(
            monthUsers,
            halfYearUsers,
            localPostCount,
            userCount,
            peerCount);
    }
}