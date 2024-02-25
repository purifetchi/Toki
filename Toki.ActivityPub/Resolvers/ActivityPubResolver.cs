using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityStreams.Objects;
using Toki.HTTPSignatures;

namespace Toki.ActivityPub.Resolvers;

/// <summary>
/// An ActivityPub resolver.
/// </summary>
public class ActivityPubResolver(
    IHttpClientFactory clientFactory,
    SignedHttpClient signedHttpClient,
    InstanceActorResolver instanceActorResolver,
    IOptions<InstanceConfiguration> opts,
    ILogger<ActivityPubResolver> logger)
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

        logger.LogInformation($"Fetching object {obj.Id}");
        
        var resp = opts.Value.SignedFetch ? 
            await FetchWithSigning(obj.Id) : 
            await FetchWithoutSigning(obj.Id);

        if (!resp.IsSuccessStatusCode)
            return null;
        
        logger.LogInformation($"{obj.Id} OK");

        return await JsonSerializer.DeserializeAsync<TAsObject>(
            await resp.Content.ReadAsStreamAsync());
    }

    /// <summary>
    /// Fetches an URL without signing the request.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The response message.</returns>
    private async Task<HttpResponseMessage> FetchWithoutSigning(string url)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new(url),
            Method = HttpMethod.Get,
            Headers =
            {
                { "Accept", "application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"" } 
            }
        };

        var client = clientFactory.CreateClient();
        return await client.SendAsync(request);
    }

    /// <summary>
    /// Fetches an URL with signing the request.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The response message.</returns>
    private async Task<HttpResponseMessage> FetchWithSigning(string url)
    {
        var keypair = await instanceActorResolver.GetInstanceActorKeypair();
        return await signedHttpClient
            .WithKey($"https://{opts.Value.Domain}/actor#key", keypair.PrivateKey!)
            .WithHeader("User-Agent", 
                $"Toki ({opts.Value.Domain}; <{opts.Value.ContactEmail}>)")
            .AddHeaderToSign("Host")
            .AddHeaderToSign("Date", 
                DateTimeOffset.UtcNow.AddSeconds(5).ToString("D, d M Y H:i:s T"))
            .WithHeader("Accept",
                "application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"")
            .Get(url);
    }
}