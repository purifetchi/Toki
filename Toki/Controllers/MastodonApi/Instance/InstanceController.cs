using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.MastodonApi.Schemas.Responses.Instance;

namespace Toki.Controllers.MastodonApi.Instance;

/// <summary>
/// The controller for the "/api/vx/instance" endpoint
/// </summary>
[ApiController]
[Route("/api")]
[EnableCors("MastodonAPI")]
public class InstanceController(
    IOptions<InstanceConfiguration> opts) : ControllerBase
{
    /// <summary>
    /// Fetches the v1 version of the information.
    /// </summary>
    /// <returns>The instance information data.</returns>
    [HttpGet]
    [Route("v1/instance")]
    public async Task<IActionResult> InstanceV1()
    {
        var config = opts.Value;

        var info = new InstanceInformationV1Response()
        {
            Uri = config.Domain,
            Email = config.ContactEmail,
            Title = "Toki server", // TODO: Make this configurable
            Version = "dev",
            ShortDescription = config.Info,
            
            Configuration = new InstanceInformationV1Configuration()
            {
                Statuses = new InstanceInformationStatuses()
            }
        };
        
        return Ok(info);
    }
}