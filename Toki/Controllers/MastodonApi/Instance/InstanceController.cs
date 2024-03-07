using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.Configuration;
using Toki.MastodonApi.Schemas.Responses.Instance;
using Toki.Services.Drive;

namespace Toki.Controllers.MastodonApi.Instance;

/// <summary>
/// The controller for the "/api/vx/instance" endpoint
/// </summary>
[ApiController]
[Route("/api")]
[EnableCors("MastodonAPI")]
public class InstanceController(
    IOptions<InstanceConfiguration> opts,
    IOptions<UploadConfiguration> uploadOpts) : ControllerBase
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

        var version = $"{opts.Value.Software.SoftwareName} {config.Software.SoftwareVersion ?? $"{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit}"}";
        
        var info = new InstanceInformationV1Response()
        {
            Uri = config.Domain,
            Email = config.ContactEmail,
            Title = config.Name,
            Version = $"4.0.0 (compatible; {version})",
            ShortDescription = config.Info,
            
            Configuration = new InstanceInformationV1Configuration()
            {
                Statuses = new InstanceInformationStatuses(),
                MediaAttachments = new InstanceInformationMediaAttachments
                {
                    SupportedMimeTypes = DriveService.AcceptedMIMEs,
                    ImageSizeLimit = uploadConfig.MaxFileSize,
                    VideoSizeLimit = uploadConfig.MaxFileSize
                }
            }
        };
        
        return Ok(info);
    }
}