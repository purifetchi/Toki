using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;

namespace Toki.ActivityPub.NodeInfo;

/// <summary>
/// A resolver for nodeinfo data.
/// </summary>
/// <param name="clientFactory">The http client factory.</param>
public class NodeInfoResolver(
    IHttpClientFactory clientFactory,
    IOptions<InstanceConfiguration> opts,
    ILogger<NodeInfoResolver> logger)
{
    /// <summary>
    /// Creates a GET request that additionally adds the Toki user agent.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <typeparam name="T">The type we're getting.</typeparam>
    /// <returns>The deserialized value if the request didn't fail.</returns>
    private async Task<T?> HttpGet<T>(string url)
        where T : class
    {
        var client = clientFactory.CreateClient();
        var message = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Headers =
            {
                { "User-Agent", opts.Value.UserAgent },
                { "Accept", "application/json" }
            }
        };

        var resp = await client.SendAsync(message);
        if (!resp.IsSuccessStatusCode)
            return null;

        // Check if we actually got a json response.
        if (resp.Content.Headers.ContentType?.ToString().Contains("json") != true)
            return null;

        return await resp.Content.ReadFromJsonAsync<T>();
    }
    
    /// <summary>
    /// Gets the node info data for an instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>The node info response, if one exists.</returns>
    public async Task<NodeInfoResponse?> Get(string instance)
    {
        var url = await FetchNodeInfoLink(
            instance,
            preferredVersion: "2.1");
        
        if (url is null)
            return null;
        
        // NOTE: I've seen at least one instance in the wild that doesn't attach a schema.
        //       Let's forcibly add one ourselves.
        if (!url.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            url = $"https://{url}";
        
        logger.LogInformation($"Found nodeinfo link for instance {instance}: {url}");

        return await HttpGet<NodeInfoResponse>(url);
    }

    /// <summary>
    /// Fetches the actual link to the nodeinfo resource.
    /// </summary>
    /// <param name="instance">The domain of the instance.</param>
    /// <param name="preferredVersion">The version of nodeinfo we're preferring.</param>
    /// <returns>The link, if one exists.</returns>
    private async Task<string?> FetchNodeInfoLink(
        string instance,
        string preferredVersion = "2.1")
    {
        const string endpoint = "/.well-known/nodeinfo";
        const string diasporaPrefix = "http://nodeinfo.diaspora.software/ns/schema";

        var nodeInfoUrl = $"https://{instance}{endpoint}";
        var client = clientFactory.CreateClient();

        var versionSelector = await HttpGet<NodeInfoVersionSelectorResponse>(nodeInfoUrl);
        if (versionSelector is null)
            return null;

        var preferred = versionSelector.Links?
            .FirstOrDefault(l => l.Relative.EndsWith(preferredVersion));

        if (preferred is not null)
            return preferred.Hyperlink;

        return versionSelector.Links?
            .Where(link => link.Relative.StartsWith(diasporaPrefix))
            .MaxBy(link => link.Hyperlink)?
            .Hyperlink;
    }
}