using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.NodeInfo;

namespace Toki.Controllers;

/// <summary>
/// The nodeinfo controller.
/// </summary>
[ApiController]
[Route("nodeinfo")]
public class NodeInfoController(
    IOptions<InstanceConfiguration> opts) : ControllerBase
{
    /// <summary>
    /// Returns the 2.1 version of the nodeinfo data.
    /// </summary>
    /// <returns>The node info response.</returns>
    [HttpGet]
    [Route("2.1")]
    [EnableCors("MastodonAPI")]
    public NodeInfoResponse NodeInfo21()
    {
        var config = opts.Value;
        
        var software = new NodeInfoSoftware
        {
            Name = config.Software.SoftwareName,
            Version = config.Software.SoftwareVersion ?? 
                $"{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit}",
            
            Repository = config.Software.SoftwareRepository,
            Homepage = config.Software.SoftwareWebsite
        };

        var metadata = new NodeInfoMetadata
        {
            Name = config.Name,
            Description = config.Info
        };
        
        return new NodeInfoResponse
        {
            Version = "2.1",
            Software = software,
            Metadata = metadata,
            
            OpenRegistrations = config.OpenRegistrations,
            Protocols =
            [
                "activitypub"
            ]
        };
    }
}