using System.Text.Json;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Resolvers;

/// <summary>
/// An ActivityPub resolver.
/// </summary>
public class ActivityPubResolver(
    IHttpClientFactory clientFactory)
{
    /// <summary>
    /// Fetches the proper ASObject from a given unresolved ASObject. 
    /// </summary>
    /// <param name="obj">The unresolved object.</param>
    /// <returns>The fetched resolved object.</returns>
    public async Task<TAsObject?> Fetch<TAsObject>(ASObject obj)
        where TAsObject: ASObject
    {
        // If this object is already resolved, no point in us inquiring about it again.
        if (obj.IsResolved && obj is TAsObject asObject)
            return asObject;

        var request = new HttpRequestMessage()
        {
            RequestUri = new(obj.Id),
            Method = HttpMethod.Get,
            Headers =
            {
                { "Accept", "application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"" } 
            }
        };

        var client = clientFactory.CreateClient();
        var resp = await client.SendAsync(request);

        if (!resp.IsSuccessStatusCode)
            return null;

        return await JsonSerializer.DeserializeAsync<TAsObject>(
            await resp.Content.ReadAsStreamAsync());
    }
}