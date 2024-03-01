using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
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
    PostManagementService postManagementService,
    PostRepository postRepo,
    WebFingerResolver webFingerResolver,
    ActivityPubResolver apResolver) : ControllerBase
{
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
    [Route("test_post")]
    public async Task<IActionResult> Post(
        [FromQuery] string actor,
        [FromQuery] string body,
        [FromQuery] PostVisibility visibility)
    {
        var u = await repo.FindByHandle(actor);
        if (u is null || u.IsRemote)
            return NotFound();

        var post = await postManagementService.Create(u, body, visibility);
        if (post is null)
            return BadRequest();
        
        return Ok(post.Id);
    }
    
    [HttpGet]
    [Route("test_boost")]
    public async Task<IActionResult> Boost(
        [FromQuery] string actor,
        [FromQuery] string id)
    {
        var u = await repo.FindByHandle(actor);
        if (u is null || u.IsRemote)
            return NotFound();

        var post = await postRepo.FindByRemoteId(id);
        if (post is null)
            return NotFound();
        
        await postManagementService.Boost(u, post);
        
        return Ok(post.Id);
    }
    
    /// <summary>
    /// Tests following a user.
    /// </summary>
    /// <param name="usId">Us.</param>
    /// <param name="themId">Them.</param>
    [HttpGet]
    [Route("test_follow")]
    public async Task<IActionResult> Follow(
        [FromQuery] string usId,
        [FromQuery] string themId)
    {
        // TODO: Actor resolution should be implemented elsewhere.
        User? actor;
        if (themId.Contains('@'))
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

            actor = await repo.FindByRemoteId(actorData.Id) ??
                        await repo.ImportFromActivityStreams(actorData);
        }
        else
        {
            actor = await repo.FindByHandle(themId);
        }

        var us = await repo.FindByHandle(usId);
        if (us is null || actor is null)
            return NotFound();
        
        await relationService.RequestFollow(
            us,
            actor);

        return Ok();
    }
}