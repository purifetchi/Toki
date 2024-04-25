using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Cryptography;
using Toki.ActivityPub.Jobs.Federation;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;
using Toki.ActivityStreams.Objects;
using Toki.Extensions;
using Toki.Services.Timelines;

namespace Toki.Controllers;

/// <summary>
/// The users controller for ActivityPub interoperability.
/// </summary>
[ApiController]
[Route("users/{handle}")]
public class UsersController(
    UserRepository repo,
    FollowRepository followRepo,
    PostRepository postRepo,
    UserRenderer renderer,
    PostRenderer postRenderer,
    InstancePathRenderer pathRenderer,
    ActivityPubMessageValidationService validator,
    TimelineBuilder timelineBuilder)
    : ControllerBase
{
    /// <summary>
    /// Gets a user's actor by their handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>The user if they exist, or nothing.</returns>
    [HttpGet]
    [Route("")]
    [Produces("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/json", "application/activity+json")]
    public async Task<ActionResult<ASActor?>> FetchActor([FromRoute] string handle)
    {
        var user = await repo.FindByHandle(handle);
        if (user is null)
            return NotFound();

        return await renderer.RenderFullActorFrom(user);
    }
    
    /// <summary>
    /// Gets the followers for a user.
    /// </summary>
    /// <param name="handle">The handle of the user.</param>
    /// <returns>The collection.</returns>
    [HttpGet]
    [Route("followers")]
    [Produces("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/json", "application/activity+json")]
    public async Task<ActionResult<ASOrderedCollection<ASObject>>> Followers([FromRoute] string handle)
    {
        // TODO: Implement pagination.
        
        var user = await repo.FindByHandle(handle);
        if (user is null)
            return NotFound();

        var followersEnumerable = await followRepo
            .GetFollowingFor(user)
            .Project<ActivityPub.Models.User>(f => f.Follower)
            .ToList();
        
        var followers = followersEnumerable.Select(follower =>
                ASObject.Link(follower.RemoteId ?? pathRenderer.GetPathToActor(follower)))
            .ToList();
        
        return new ASOrderedCollection<ASObject>()
        {
            Id = $"{pathRenderer.GetPathToActor(user)}/followers",

            OrderedItems = followers,
            TotalItems = followers.Count
        };
    }
    
    /// <summary>
    /// Gets the followed actors for a user.
    /// </summary>
    /// <param name="handle">The handle of the user.</param>
    /// <returns>The collection.</returns>
    [HttpGet]
    [Route("following")]
    [Produces("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/json", "application/activity+json")]
    public async Task<ActionResult<ASOrderedCollection<ASObject>>> Following([FromRoute] string handle)
    {
        // TODO: Implement pagination.
        
        var user = await repo.FindByHandle(handle);
        if (user is null)
            return NotFound();

        var followingEnumerable = await followRepo
            .GetFollowingFor(user)
            .Project<ActivityPub.Models.User>(f => f.Followee)
            .ToList();
        
        var following = followingEnumerable.Select(follower =>
                ASObject.Link(follower.RemoteId ?? pathRenderer.GetPathToActor(follower)))
            .ToList();
        
        return new ASOrderedCollection<ASObject>()
        {
            Id = $"{pathRenderer.GetPathToActor(user)}/following",

            OrderedItems = following,
            TotalItems = following.Count
        };
    }

    /// <summary>
    /// Gets the featured notes for an actor.
    /// </summary>
    /// <param name="handle">The handle of the actor.</param>
    /// <returns>The collection.</returns>
    [HttpGet]
    [Route("collections/featured")]
    [Produces("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/json",
        "application/activity+json")]
    public async Task<ActionResult<ASOrderedCollection<ASObject>>> Featured(
        [FromRoute] string handle)
    {
        // TODO: Implement pagination.
        var user = await repo.FindByHandle(handle);
        if (user is null)
            return NotFound();

        var pinned = await postRepo.FindPinnedPostsForUser(user);

        var notes = new List<ASObject>();
        foreach (var note in pinned)
        {
            notes.Add(
                await postRenderer.RenderFullNoteFrom(note.Post));
        }
        
        return new ASOrderedCollection<ASObject>()
        {
            Id = $"{pathRenderer.GetPathToActor(user)}/collections/featured",

            OrderedItems = notes,
            TotalItems = notes.Count
        };
    }
    
    /// <summary>
    /// The user inbox.
    /// </summary>
    /// <param name="handle">The receiving user's handle.</param>
    /// <param name="asObject">The activity.</param>
    /// <returns>The result.</returns>
    [HttpPost]
    [Consumes("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/activity+json")]
    [Route("inbox")]
    public async Task<IActionResult> Inbox(
        [FromRoute] string handle,
        [FromBody] ASObject? asObject)
    {
        var validation = await validator.Validate(HttpContext.Request.ToTokiHttpRequest(), asObject);
        switch (validation)
        {
            case MessageValidationResponse.Ok:
                // TODO: This is really ugly.
                var data = JsonSerializer.Serialize(asObject);
                BackgroundJob.Enqueue<InboxHandlerJob>(job =>
                    job.HandleActivity(data));
        
                return Accepted();
            
            // We will fake accepting this one regardless, just because Mastodon will keep spamming us those
            // forever.
            case MessageValidationResponse.MastodonDeleteForUnknownUser:
                return Accepted();
            
            case MessageValidationResponse.InvalidActor:
            case MessageValidationResponse.CannotParseSignature:
            case MessageValidationResponse.NotActivity:
                return BadRequest();

            case MessageValidationResponse.KeyIdMismatch:
            case MessageValidationResponse.ValidationFailed:
            case MessageValidationResponse.GenericError:
            default:
                return Unauthorized();
        }
    }
    
    /// <summary>
    /// Gets the user's outbox.
    /// </summary>
    /// <param name="handle">The user's handle.</param>
    /// <returns>The activities produced by the user.</returns>
    [HttpGet]
    [Produces("application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"", "application/activity+json")]
    [Route("outbox")]
    public async Task<ActionResult<ASOrderedCollection<ASObject>>> Outbox(
        [FromRoute] string handle)
    {
        var user = await repo.FindByHandle(handle);
        if (user is null)
            return NotFound();
        
        var posts = await timelineBuilder
            .ViewAs(null)
            .Filter(post => post.AuthorId == user.Id)
            .GetTimeline();

        var items = new List<ASObject>();
        foreach (var post in posts)
        {
            items.Add(
                await postRenderer.RenderCreationForNote(post));
        }
        
        return new ASOrderedCollection<ASObject>()
        {
            Id = $"{pathRenderer.GetPathToActor(user)}/outbox",

            OrderedItems = items,
            TotalItems = items.Count
        };;
    }
}