using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityStreams.Activities;
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
    /// The serializer options for reading fetched objects.
    /// </summary>
    private static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
    
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
        
        // We've received a bogus object, no idea how it happens, but it does.
        if (string.IsNullOrEmpty(obj.Id))
            return null;

        logger.LogInformation($"Fetching object {obj.Id}");

        HttpResponseMessage resp;
        try
        {
            resp = opts.Value.SignedFetch ? 
                await FetchWithSigning(obj.Id) : 
                await FetchWithoutSigning(obj.Id);
        }
        catch (TaskCanceledException e)
        {
            logger.LogWarning($"Cancelled task while waiting on {obj.Id}, possibly timed out.");
            return null;
        }
        catch (HttpRequestException e)
        {
            logger.LogWarning($"Request exception while fetching object {obj.Id}! {e.Message}");
            return null;
        }

        if (!resp.IsSuccessStatusCode)
            return null;

        if (resp.Content.Headers.ContentType?.MediaType != null &&
            resp.Content.Headers.ContentType.MediaType.Contains("json", StringComparison.InvariantCultureIgnoreCase) == false)
        {
            logger.LogWarning($"Object {obj.Id} didn't return a JSON response! [{resp.Content.Headers.ContentType.MediaType}]");
            return null;
        }
        
        logger.LogInformation($"{obj.Id} OK");

        var deserialized = await JsonSerializer.DeserializeAsync<TAsObject>(
            await resp.Content.ReadAsStreamAsync(),
            options: SerializerOptions);

        if (deserialized is null)
            return null;

        if (deserialized.Id != obj.Id)
        {
            logger.LogWarning($"Someone tried to impersonate another id! Requested '{obj.Id}' and got '{deserialized.Id}'.");
            return null;
        }

        if (deserialized is ASActivity activity)
        {
            var reqHost = new Uri(obj.Id)
                .Host;

            var actorHost = new Uri(activity.Actor.Id)
                .Host;

            if (reqHost != actorHost)
            {
                logger.LogWarning($"Someone tried to impersonate an actor? Activity was on host '{reqHost}', while actor was on host '{actorHost}'.");
                
                // TODO: I want to return null here but I worry about split domains...
            }
        }

        return deserialized;
    }

    /// <summary>
    /// Fetches all of the objects from a collection.
    /// </summary>
    /// <param name="obj">The fetched collection.</param>
    /// <returns>A list of the objects.</returns>
    public async Task<IList<ASObject>?> FetchCollection(ASObject obj)
    {
        // NOTE: This was chosen pretty much arbitrarily
        const int maxFetchedPages = 50;
        
        var maybeCollection = await Fetch<ASObject>(obj);
        var fetchedPages = 0;
        
        switch (maybeCollection)
        {
            case ASOrderedCollection<ASObject> { First: null } orderedCollection:
                return orderedCollection.OrderedItems;
            case ASCollection<ASObject> { First: null } collection:
                return collection.Items;
            case ASOrderedCollection<ASObject> { First: { } first }:
            {
                var page = await Fetch<ASOrderedCollectionPage<ASObject>>(first);
                var items = new List<ASObject>();
                while (page is not null &&
                       fetchedPages < maxFetchedPages)
                {
                    fetchedPages++;
                    items.AddRange(page.OrderedItems);
                    if (page.Next is null)
                        break;
                    
                    if (page.Next.StartsWith(page.Id, StringComparison.InvariantCultureIgnoreCase))
                        break;
                    
                    page = await Fetch<ASOrderedCollectionPage<ASObject>>(
                        ASObject.Link(page.Next));
                }

                return items;
            }
            case ASCollection<ASObject> { First: { } first }:
            {
                var page = await Fetch<ASCollectionPage<ASObject>>(first);
                var items = new List<ASObject>();
                while (page is not null &&
                       fetchedPages < maxFetchedPages)
                {
                    fetchedPages++;
                    items.AddRange(page.Items);
                    if (page.Next is null)
                        break;

                    if (page.Next.StartsWith(page.Id, StringComparison.InvariantCultureIgnoreCase))
                        break;
                    
                    page = await Fetch<ASCollectionPage<ASObject>>(
                        ASObject.Link(page.Next));
                }

                return items;
            }
            default:
                return null;
        }
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
                { "Accept", "application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"" },
                { "User-Agent", opts.Value.UserAgent }
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
            .NewRequest()
            .WithKey($"https://{opts.Value.Domain}/actor#key", keypair.PrivateKey!)
            .WithHeader("User-Agent", opts.Value.UserAgent)
            .AddHeaderToSign("Host")
            .SetDate(DateTimeOffset.UtcNow.AddSeconds(5))
            .WithHeader("Accept",
                "application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"")
            .Get(url);
    }
}