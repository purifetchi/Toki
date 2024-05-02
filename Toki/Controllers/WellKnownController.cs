using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.NodeInfo;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.WebFinger;

namespace Toki.Controllers;

/// <summary>
/// The controller for the .well-known route.
/// </summary>
[ApiController]
[Route(".well-known")]
public class WellKnownController(
    WebFingerRenderer renderer,
    IOptions<InstanceConfiguration> opts)
    : ControllerBase
{
    /// <summary>
    /// Runs a webfinger query.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <returns>Either a webfinger response, or nothing.</returns>
    [HttpGet]
    [Route("webfinger")]
    [EnableCors("MastodonAPI")]
    public async Task<ActionResult<WebFingerResponse?>> WebFinger([FromQuery] string resource)
    {
        // We're dealing with resolution on the AP actor id.
        if (resource.StartsWith($"https://{opts.Value.Domain}"))
        {
            // TODO: This should be optimized but w/e.
            
            // We're dealing with the instance actor.
            if (resource == $"https://{opts.Value.Domain}/actor")
                return await renderer.FindUser($"acct:{InstanceActorResolver.INSTANCE_ACTOR_NAME}@{opts.Value.Domain}");

            var parts = resource.Split('/');

            // Ensure we're fingering users.
            if (parts[^2] != "users")
                return BadRequest();
            
            var handle = parts.Last();
            var apUser = await renderer.FindUser($"acct:{handle}@{opts.Value.Domain}");
            if (apUser is null)
                return NotFound();
                
            return apUser;
        }

        var resp = await renderer.FindUser(resource);
        if (resp is null)
            return NotFound();

        return resp;
    }

    /// <summary>
    /// Fetches the node info versions for this server.
    /// </summary>
    /// <returns>The node info version selector.</returns>
    [HttpGet]
    [Route("nodeinfo")]
    [EnableCors("MastodonAPI")]
    public NodeInfoVersionSelectorResponse NodeInfoVersions()
    {
        return new NodeInfoVersionSelectorResponse()
        {
            Links =
            [
                new()
                {
                    Relative = "http://nodeinfo.diaspora.software/ns/schema/2.1",
                    Hyperlink = $"https://{opts.Value.Domain}/nodeinfo/2.1"
                },
                new()
                {
                    Relative = "http://nodeinfo.diaspora.software/ns/schema/2.0",
                    Hyperlink = $"https://{opts.Value.Domain}/nodeinfo/2.0"
                },
                new()
                {
                    Relative = "https://www.w3.org/ns/activitystreams#Application",
                    Hyperlink = $"https://{opts.Value.Domain}/actor"
                }
            ]
        };
    }

    /// <summary>
    /// Fetches the host meta for this instance.
    /// </summary>
    /// <returns>The host meta response.</returns>
    [HttpGet]
    [Route("host-meta")]
    [EnableCors("MastodonAPI")]
    public IResult HostMeta()
    {
        return Results.Content($$"""
<?xml version="1.0" encoding="UTF-8"?>
<XRD xmlns="http://docs.oasis-open.org/ns/xri/xrd-1.0">
   <Link type="application/xrd+xml" template="https://{{opts.Value.Domain}}/.well-known/webfinger?resource={uri}" rel="lrdd" />
</XRD>
""", "application/xrd+xml");
    }
}