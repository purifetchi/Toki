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
    ILogger<NodeInfoResolver> logger)
{
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
        
        logger.LogInformation($"Found nodeinfo link for instance {instance}: {url}");

        var client = clientFactory.CreateClient();
        return await client.GetFromJsonAsync<NodeInfoResponse>(url);
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

        var versionSelector = await client.GetFromJsonAsync<NodeInfoVersionSelectorResponse>(nodeInfoUrl);
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