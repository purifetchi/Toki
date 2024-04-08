using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Emojis;
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
    DriveService drive,
    EmojiService emoji,
    ActivityPubResolver resolver) : ControllerBase
{
    [HttpPost]
    [Route("test_create_emoji")]
    public async Task<IActionResult> CreateEmoji(
        [FromQuery] string shortcode,
        [FromForm] IFormFile file)
    {
        var link = await drive.Store(file);
        if (link is null)
            return BadRequest();

        var emo = await emoji.CreateLocalEmoji(
            shortcode,
            link);
        
        return Ok(emo?.Id);
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

    [HttpGet]
    [Route("test_fetch_collection")]
    public async Task<IActionResult> FetchCollection(
        [FromQuery] string url)
    {
        var coll = await resolver.FetchCollection(ASObject.Link(url));
        if (coll is null)
            return NotFound();
        
        return Ok(coll);
    }
}