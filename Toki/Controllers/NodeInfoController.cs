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
    /// Returns the 2.1 version of the nodeinfo data.
    /// </summary>
    /// <returns>The node info response.</returns>
    [HttpGet]
    [Route("2.1")]
    [EnableCors("MastodonAPI")]
    public async Task<NodeInfoResponse> NodeInfo21()
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
            Version = "2.1",
            Software = software,
            Metadata = metadata,
            
            OpenRegistrations = config.OpenRegistrations,
            Usage = usage,
            Protocols =
            [
                "activitypub"
            ]
        };
    }
}