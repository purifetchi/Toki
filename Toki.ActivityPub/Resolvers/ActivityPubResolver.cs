using System.Net.Http.Headers;
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
    /// The types we accept when fetching.
    /// </summary>
    private const string ACCEPT_TYPES 
        = "application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\", application/activity+json";
    
    /// <summary>
    /// The serializer options for reading fetched objects.
    /// </summary>
    private static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    /// <summary>
    /// Checks if the given type is a valid ActivityPub Content-Type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Whether it is an ActivityPub content type.</returns>
    private static bool IsActivityPubContentType(string type) => type switch
    {
        "application/activity+json" => true,
        "application/ld+json" => true,
        _ => false
    };

    /// <summary>
    /// Fetches an object without ActivityStreams object validation.
    /// </summary>
    /// <param name="uri">The uri of the object to fetch.</param>
    /// <typeparam name="TAsObject">The type to fetch.</typeparam>
    /// <returns>The fetched object on success, null otherwise.</returns>
    private async Task<TAsObject?> FetchWithoutActivityStreamsValidation<TAsObject>(Uri uri)
        where TAsObject : ASObject
    {
        // The max response size, in bytes. (10MB by default)
        const int maxResponseSize = 5 * 1024 * 1024;
        
        // If we're fetching an object from our own domain, we're doing something very wrong.
        // That, or someone is trying to spoof an object.
        if (uri.Host == opts.Value.Domain)
        {
            logger.LogWarning($"Prevented fetching local domain object {uri}.");
            return null;
        }
        
        logger.LogInformation($"Fetching object {uri}");
        
        HttpResponseMessage resp;
        try
        {
            resp = opts.Value.SignedFetch ? 
                await FetchWithSigning(uri) : 
                await FetchWithoutSigning(uri);
        }
        catch (TaskCanceledException e)
        {
            logger.LogWarning($"Cancelled task while waiting on {uri}, possibly timed out.");
            return null;
        }
        catch (HttpRequestException e)
        {
            logger.LogWarning($"Request exception while fetching object {uri}! {e.Message}");
            return null;
        }

        if (!resp.IsSuccessStatusCode)
            return null;

        var headers = resp.Content.Headers;

        // Check that the content type is something we actually care about.
        if (headers.ContentType?.MediaType != null)
        {
            var types = headers
                .ContentType
                .ToString()
                .Split(',')
                .Select(MediaTypeWithQualityHeaderValue.Parse)
                .Select(t => t.MediaType!);

            if (!types.Any(IsActivityPubContentType))
            {
                logger.LogWarning($"Object {uri} didn't return a JSON response! [{headers.ContentType.MediaType}]");
                return null;
            }
        }

        // Check that the content length is sane.
        if (headers.ContentLength > maxResponseSize)
        {
            logger.LogWarning($"Object {uri} had a length that's too big! [{headers.ContentLength}]");
            return null;
        }
        
        logger.LogInformation($"{uri} OK");
    
        return await JsonSerializer.DeserializeAsync<TAsObject>(
            await resp.Content.ReadAsStreamAsync(),
            options: SerializerOptions);
    }
    
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

        var uri = new Uri(obj.Id);
        
        // Fetch the object without ASObject validation.
        var deserialized = await FetchWithoutActivityStreamsValidation<TAsObject>(uri);
        if (deserialized is null)
            return null;

        // Check if the deserialized ID and the object ID are the same.
        if (deserialized.Id != obj.Id)
        {
            // If they aren't try to fetch from deserialized.id again, to ensure obj.id wasn't an alias...
            // This is used by Mastodon and seems to work.
            var objectFromId = await FetchWithoutActivityStreamsValidation<TAsObject>(
                new Uri(deserialized.Id));

            if (objectFromId?.Id != deserialized.Id)
            {
                logger.LogWarning($"Someone tried to impersonate another id! Requested '{obj.Id}' and got '{deserialized.Id}'.");
                return null;
            }
        }

        if (deserialized is ASActivity activity)
        {
            var reqHost = new Uri(deserialized.Id)
                .Host;

            var actorHost = new Uri(activity.Actor.Id)
                .Host;

            if (reqHost != actorHost)
            {
                logger.LogWarning($"Someone tried to impersonate an actor? Activity was on host '{reqHost}', while actor was on host '{actorHost}'.");
                return null;
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
    /// <param name="uri">The url.</param>
    /// <returns>The response message.</returns>
    private async Task<HttpResponseMessage> FetchWithoutSigning(Uri uri)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = uri,
            Method = HttpMethod.Get,
            Headers =
            {
                { "Accept", ACCEPT_TYPES },
                { "User-Agent", opts.Value.UserAgent }
            }
        };

        var client = clientFactory.CreateClient();
        return await client.SendAsync(request);
    }

    /// <summary>
    /// Fetches an URL with signing the request.
    /// </summary>
    /// <param name="uri">The url.</param>
    /// <returns>The response message.</returns>
    private async Task<HttpResponseMessage> FetchWithSigning(Uri uri)
    {
        var keypair = await instanceActorResolver.GetInstanceActorKeypair();
        return await signedHttpClient
            .NewRequest()
            .WithKey($"https://{opts.Value.Domain}/actor#key", keypair.PrivateKey!)
            .WithHeader("User-Agent", opts.Value.UserAgent)
            .AddHeaderToSign("Host")
            .SetDate(DateTimeOffset.UtcNow.AddSeconds(5))
            .WithHeader("Accept", ACCEPT_TYPES)
            .Get(uri);
    }
}