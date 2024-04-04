using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Toki.Extensions;

/// <summary>
/// Extensions for the <see cref="IDistributedCache"/> interface.
/// </summary>
public static class IDistributedCacheExtensions
{
    /// <summary>
    /// Gets and deserializes an item from the cache based on the key.
    /// </summary>
    /// <param name="cache">The cache.</param>
    /// <param name="key">The key of the item.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The item.</returns>
    public static async Task<T?> GetAsync<T>(
        this IDistributedCache cache, 
        string key)
    {
        var data = await cache.GetStringAsync(key);
        return data is null ? 
            default : 
            JsonSerializer.Deserialize<T>(data);
    }

    /// <summary>
    /// Serializes and sets an item in the cache based on a key.
    /// </summary>
    /// <param name="cache">The cache.</param>
    /// <param name="key">The key of the item.</param>
    /// <param name="value">The value of the item.</param>
    /// <param name="opts">The additional options.</param>
    /// <typeparam name="T">The type.</typeparam>
    public static async Task SetAsync<T>(
        this IDistributedCache cache, 
        string key,
        T value, 
        DistributedCacheEntryOptions? opts = null)
    {
        var data = JsonSerializer.Serialize(value);
        if (opts is not null)
            await cache.SetStringAsync(key, data, opts);
        else
            await cache.SetStringAsync(key, data);
    }
}