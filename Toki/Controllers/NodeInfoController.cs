using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.NodeInfo;
using Toki.Services.Usage;

namespace Toki.Controllers;

/// <summary>
/// The nodeinfo controller.
/// </summary>
[ApiController]
[Route("nodeinfo")]
public class NodeInfoController(
    UsageService usageService,
    IOptions<InstanceConfiguration> opts) : ControllerBase
{
    /// <summary>
    /// The list of supported operations by this server.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _supportedOperations = new Dictionary<string, IReadOnlyList<string>>()
    {
        { "com.shinolabs.api.bite", ["1.0.0"] },
        { "jetzt.mia.ns.activitypub.accept.bite", ["1.0.0"]}
    };
    
    /// <summary>
    /// Creates a node info response for a given version.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns>The resulting node info response.</returns>
    private async Task<NodeInfoResponse> GetNodeInfoResponse(
        string version)
    {
        var config = opts.Value;
        
        var software = new NodeInfoSoftware
        {
            Name = config.Software.SoftwareName,
            Version = config.Software.SoftwareVersion!,
            
            Repository = config.Software.SoftwareRepository,
            Homepage = config.Software.SoftwareWebsite
        };

        var metadata = new NodeInfoMetadata
        {
            Name = config.Name,
            Description = config.Info
        };

        var stats = await usageService.GetStatistics();
        var usage = new NodeInfoUsage()
        {
            Users = new NodeInfoUsageUsers()
            {
                Total = stats.UserCount,
                ActiveHalfYear = stats.ActiveThisHalfYear,
                ActiveMonth = stats.ActiveThisMonth
            },
            LocalPosts = stats.LocalPosts
        };
        
        return new NodeInfoResponse
        {
            Version = version,
            Software = software,
            Metadata = metadata,
            
            OpenRegistrations = config.OpenRegistrations,
            Usage = usage,
            Protocols =
            [
                "activitypub"
            ],
            
            Operations = _supportedOperations
        };
    }
    
    /// <summary>
    /// Returns the 2.1 version of the nodeinfo data.
    /// </summary>
    /// <returns>The node info response.</returns>
    [HttpGet]
    [Route("2.1")]
    [EnableCors("MastodonAPI")]
    public async Task<NodeInfoResponse> NodeInfo21()
    {
        return await GetNodeInfoResponse("2.1");
    }
    
    /// <summary>
    /// Returns the 2.0 version of the nodeinfo data.
    /// </summary>
    /// <returns>The node info response.</returns>
    [HttpGet]
    [Route("2.0")]
    [EnableCors("MastodonAPI")]
    public async Task<NodeInfoResponse> NodeInfo20()
    {
        return await GetNodeInfoResponse("2.0");
    }
}