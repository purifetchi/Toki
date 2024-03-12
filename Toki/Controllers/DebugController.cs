using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.Users;
using Toki.ActivityPub.WebFinger;
using Toki.ActivityStreams.Objects;
using Toki.Services.Drive;

namespace Toki.Controllers;

/// <summary>
/// A controller only used for testing various Toki features before we implement a sane way to interact with it.
/// </summary>
[ApiController]
[Route("/debug")]
public class DebugController(
    UserRepository repo,
    DriveService drive) : ControllerBase
{
    [HttpPost]
    [Route("test_upload_file")]
    public async Task<IActionResult> UploadFile(
        [FromForm] IFormFile file)
    {
        var link = await drive.Store(file);
        if (link is null)
            return BadRequest();

        return Ok(link);
    }

    [HttpGet]
    [Route("test_create_user")]
    public async Task<IActionResult> CreateUser(
        [FromQuery] string username,
        [FromQuery] string? password = null)
    {
        var u = await repo.CreateNewUser(username, password);
        if (u is null)
            return BadRequest();

        return Ok(u.Id);
    }
}