using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.Configuration;
using Toki.MastodonApi.Schemas.Responses.Instance;
using Toki.Services.Drive;
using Toki.Services.Usage;

namespace Toki.Controllers.MastodonApi.Instance;

/// <summary>
/// The controller for the "/api/vx/instance" endpoint
/// </summary>
[ApiController]
[Route("/api")]
[EnableCors("MastodonAPI")]
public class InstanceController(
    InstanceRepository repo,
    UsageService usageService,
    IOptions<InstanceConfiguration> opts,
    IOptions<UploadConfiguration> uploadOpts,
    IOptions<FrontendConfiguration> frontendOpts) : ControllerBase
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
        var uploadConfig = uploadOpts.Value;

        var version = $"{opts.Value.Software.SoftwareName}/{config.Software.SoftwareVersion!}";

        var stats = await usageService.GetStatistics();
        
        var info = new InstanceInformationV1Response()
        {
            Uri = config.Domain,
            Email = config.ContactEmail,
            Title = config.Name,
            Version = $"4.0.0 (compatible; {version})",
            ShortDescription = config.Info,
            Description = config.Info,
            Thumbnail = frontendOpts.Value.ThumbnailPath ?? $"https://{config.Domain}/favicon.png",
            
            Configuration = new InstanceInformationV1Configuration()
            {
                Statuses = new InstanceInformationStatuses()
                {
                    MaxCharacters = config.Limits.MaxPostCharacterLimit.ToString(),
                    MaxMediaAttachments = config.Limits.MaxAttachmentCount.ToString()
                },
                MediaAttachments = new InstanceInformationMediaAttachments
                {
                    SupportedMimeTypes = DriveService.AcceptedMIMEs,
                    ImageSizeLimit = uploadConfig.MaxFileSize,
                    VideoSizeLimit = uploadConfig.MaxFileSize,
                    
                    // NOTE: All of these are the 8K resolution, this is a test value for any soft that expect
                    //       these values to be present.
                    ImageMatrixLimit = 33177600,
                    VideoMatrixLimit = 33177600 
                },
                Polls = new InstanceInformationPolls()
            },
            
            Statistics = new InstanceInformationStatistics()
            {
                UserCount = stats.UserCount,
                StatusCount = stats.LocalPosts,
                DomainCount = stats.PeerCount
            }
        };
        
        return Ok(info);
    }

    /// <summary>
    /// Gets a list of all the connected instances.
    /// </summary>
    /// <returns>The list of connected instances.</returns>
    [HttpGet]
    [Route("v1/instance/peers")]
    [Produces("application/json")]
    public async Task<IEnumerable<string>> GetPeers()
    {
        var instances = await repo.GetAllConnectedInstances();
        return instances
            .Select(inst => inst.Domain);
    }
}