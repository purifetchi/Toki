using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.Binding;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Objects;
using Toki.MastodonApi.Schemas.Requests.Statuses;
using Toki.Middleware.OAuth2;
using Toki.Services.Drive;

namespace Toki.Controllers.MastodonApi.Statuses;

/// <summary>
/// Controller for the "/api/vx/media" endpoints.
/// </summary>
[ApiController]
[Route("/api")]
[EnableCors("MastodonAPI")]
public class MediaController(
    DriveService drive,
    PostRepository postRepository,
    StatusRenderer statusRenderer) : ControllerBase
{
    /// <summary>
    /// Creates a media attachment to be used with a new status.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The resulting <see cref="MediaAttachment"/>, or nothing.</returns>
    [HttpPost]
    [Route("v1/media")]
    [Route("v2/media")]
    [Produces("application/json")]
    [OAuth("write:media")]
    public async Task<ActionResult<MediaAttachment>> UploadMedia(
        [FromForm] PostMediaRequest request)
    {
        if (request.File is null)
            return BadRequest(new MastodonApiError("No file supplied."));
        
        // TODO: We can process the media asynchronously here... But for now we don't need to.
        var path = await drive.Store(request.File);
        if (path is null)
            return UnprocessableEntity(new MastodonApiError("Invalid file."));

        var attachment = await postRepository.CreateDetachedAttachment(
            path,
            request.Description,
            request.File.ContentType);

        return statusRenderer.RenderAttachmentFrom(
            attachment);
    }

    /// <summary>
    /// Update a <see cref="MediaAttachment"/>’s parameters, before it is attached to a status and posted.
    /// </summary>
    /// <param name="id">The ID of the MediaAttachment in the database.</param>
    /// <param name="request">The request.</param>
    /// <returns>The resulting <see cref="MediaAttachment"/>, or nothing.</returns>
    [HttpPut]
    [Route("v2/media/{id}")]
    [Route("v1/media/{id}")]
    [Produces("application/json")]
    [OAuth("write:media")]
    public async Task<ActionResult<MediaAttachment>> UpdateMedia(
        [FromRoute] Ulid id,
        [FromHybrid] PostMediaRequest request)
    {
        var attachment = await postRepository.FindAttachmentById(id);
        if (attachment is null)
            return NotFound(new MastodonApiError("Record not found."));

        if (attachment.ParentId is not null)
            return NotFound(new MastodonApiError("Record not found."));

        attachment.Description = request.Description;
        await postRepository.UpdateAttachment(attachment);
        
        return statusRenderer.RenderAttachmentFrom(
            attachment);
    }
}