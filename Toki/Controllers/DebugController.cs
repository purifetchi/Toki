using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.Users;
using Toki.ActivityPub.WebFinger;
using Toki.ActivityStreams.Objects;

namespace Toki.Controllers;

/// <summary>
/// A controller only used for testing various Toki features before we implement a sane way to interact with it.
/// </summary>
[ApiController]
[Route("/debug")]
public class DebugController(
    UserRelationService relationService,
    UserRepository repo,
    WebFingerResolver webFingerResolver,
    ActivityPubResolver apResolver) : ControllerBase
{
    /// <summary>
    /// Tests following a user.
    /// </summary>
    /// <param name="us">Us.</param>
    /// <param name="them">Them.</param>
    [HttpGet]
    [Route("test_follow")]
    public async Task<IActionResult> Follow(
        [FromQuery] string usId,
        [FromQuery] string themId)
    {
        var resp = await webFingerResolver.FingerAtHandle(themId);
        var id = resp?.Links?
            .FirstOrDefault(l => l.Type == "application/activity+json")?
            .Hyperlink;

        if (id is null)
            return NotFound();

        var actorData = await apResolver.Fetch<ASActor>(ASObject.Link(id));
        if (actorData is null)
            return NotFound();

        var actor = await repo.FindByRemoteId(actorData.Id) ??
                    await repo.ImportFromActivityStreams(actorData);

        var us = await repo.FindByHandle(usId);
        if (us is null || actor is null)
            return NotFound();
        
        await relationService.RequestFollow(
            us,
            actor);

        return Ok();
    }
}