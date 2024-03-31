using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.DTO;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.Binding;
using Toki.Extensions;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Objects;
using Toki.MastodonApi.Schemas.Requests.Statuses;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Controllers.MastodonApi.Statuses;

/// <summary>
/// The controller for the "/api/v1/statuses" endpoint.
/// </summary>
[ApiController]
[Route("/api/v1/statuses")]
[EnableCors("MastodonAPI")]
public class StatusesController(
    PostManagementService postManagementService,
    PostRepository repo,
    StatusRenderer statusRenderer,
    AccountRenderer accountRenderer) : ControllerBase
{
    /// <summary>
    /// Posts a request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Consumes("application/json", "application/x-www-form-urlencoded")]
    [Produces("application/json")]
    [OAuth("write:statuses")]
    public async Task<IActionResult> PostStatus(
        [FromHybrid] PostStatusRequest request)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        // Check if we have anything in this post. (Do not post without any content)
        // TODO: Polls should also count.
        var hasMedia = request.MediaIds is not null &&
                       request.MediaIds.Count > 0;
        
        if (string.IsNullOrWhiteSpace(request.Status) && !hasMedia)
            return BadRequest(new MastodonApiError("Validation error: Post cannot be empty."));

        var replyingTo = Ulid.TryParse(request.InReplyTo, out var replyingGuid)
            ? await repo.FindById(replyingGuid)
            : null;

        var guids = request.MediaIds?
            .Select(Ulid.Parse)
            .ToList();

        var attachments = guids is not null ? 
            await repo.FindMultipleAttachmentsByIds(guids) : 
            null;
        
        var post = await postManagementService.Create(new PostCreationRequest
        {
            Author = user,
            Content = request.Status ?? string.Empty,
            Visibility = request.GetVisibility(),
            
            IsSensitive = request.Sensitive,
            ContentWarning = request.SpoilerText,
            InReplyTo = replyingTo,
            
            Media = attachments
        });
        
        if (post is null)
            return BadRequest(new MastodonApiError("Posting error: Cannot post status."));

        return Ok(
            statusRenderer.RenderForPost(post));
    }

    /// <summary>
    /// Fetches a status.
    /// </summary>
    /// <param name="id">The id of the status.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpGet]
    [Route("{id}")]
    [OAuth(manualScopeValidation: true)]
    [Produces("application/json")]
    public async Task<IActionResult> FetchStatus(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()?
            .User;
        
        var post = await repo.FindById(id);
        if (post is null || !post.VisibleByUser(user))
            return NotFound(new MastodonApiError("Record not found."));

        return Ok(
            await statusRenderer.RenderStatusForUser(user, post));
    }
    
    /// <summary>
    /// Deletes a status.
    /// </summary>
    /// <param name="id">The id of the status.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpDelete]
    [Route("{id}")]
    [OAuth("write:statuses")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteStatus(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()?
            .User;
        
        var post = await repo.FindById(id);
        if (post is null || post.Author != user)
            return NotFound(new MastodonApiError("Record not found."));

        // TODO: Collect and delete the local attachments
        await postManagementService.Delete(post);
        
        return Ok(
            await statusRenderer.RenderStatusForUser(user, post));
    }
    
    /// <summary>
    /// Fetches a status.
    /// </summary>
    /// <param name="id">The id of the status.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpGet]
    [Route("{id}/context")]
    [OAuth(manualScopeValidation: true)]
    [Produces("application/json")]
    public async Task<IActionResult> FetchStatusContext(
        [FromRoute] Ulid id)
    {
        // TODO: Implement the limits as described in https://docs.joinmastodon.org/methods/statuses/#context
        
        var user = HttpContext.GetOAuthToken()?
            .User;
        
        var post = await repo.FindById(id);
        if (post is null || !post.VisibleByUser(user))
            return NotFound(new MastodonApiError("Record not found."));

        if (post.Context is null)
        {
            return Ok(new Context
            {
                Parents = [await statusRenderer.RenderStatusForUser(user, post)]
            });
        }
        
        var repliesInContext = await repo.CreateCustomQuery()
            .AddMastodonRenderNecessities()
            .Where(p => p.Context == post.Context)
            .ToListAsync();
        
        // TODO: This sucks and it sucks horribly, I have no clue right now how to make it better...
        //       Iceshrimp seems to make use of a database function, from what I can tell, although
        //       I couldn't find any code related to it.
        var parents = new List<Post>();
        
        // Traverse the tree upwards.
        var temp = post;
        while (temp?.ParentId is not null)
        {
            temp = repliesInContext.FirstOrDefault(p => p.Id == temp.ParentId);
            if (temp is not null)
                parents.Insert(0, temp);
        }

        var children = new List<Post>();
        var queue = new Queue<Ulid>();
        queue.Enqueue(post.Id);

        // Traverse the tree downwards.
        while (queue.TryDequeue(out var postId))
        {
            var replies = repliesInContext.Where(p => p.ParentId == postId)
                .ToList();
            
            foreach (var reply in replies)
                queue.Enqueue(reply.Id);
            
            children.AddRange(replies);
        }
        
        return Ok(new Context
        {
            Parents = await statusRenderer.RenderManyStatusesForUser(user, parents),
            Children = await statusRenderer.RenderManyStatusesForUser(user, children)
        });
    }

    /// <summary>
    /// Favourites a post.
    /// </summary>
    /// <param name="id">The id of the post.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Produces("application/json")]
    [OAuth("write:favourites")]
    [Route("{id}/favourite")]
    public async Task<IActionResult> Favourite(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var post = await repo.FindById(id);
        if (post is null || !post.VisibleByUser(user))
            return NotFound(new MastodonApiError("Record not found."));

        await postManagementService.Like(
            user,
            post);
        
        var status = statusRenderer.RenderForPost(post);
        status.Liked = true;
        
        return Ok(status);
    }
    
    /// <summary>
    /// Remove a status from your favourites list.
    /// </summary>
    /// <param name="id">The ID of the Status in the database.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Produces("application/json")]
    [OAuth("write:favourites")]
    [Route("{id}/unfavourite")]
    public async Task<IActionResult> Unfavourite(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var post = await repo.FindById(id);
        if (post is null || !post.VisibleByUser(user))
            return NotFound(new MastodonApiError("Record not found."));

        await postManagementService.UndoLike(
            user,
            post);
        
        var status = statusRenderer.RenderForPost(post);
        status.Liked = false;
        
        return Ok(status);
    }
    
    /// <summary>
    /// Reblogs a post.
    /// </summary>
    /// <param name="id">The id of the post.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Produces("application/json")]
    [OAuth("write:favourites")]
    [Route("{id}/reblog")]
    public async Task<IActionResult> Reblog(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var post = await repo.FindById(id);
        if (post is null || !post.VisibleByUser(user))
            return NotFound(new MastodonApiError("Record not found."));

        var boost = await postManagementService.Boost(
            user,
            post);

        if (boost is null)
            return BadRequest(new MastodonApiError("Error while boosting post."));
        
        var status = statusRenderer.RenderForPost(post);
        status.Boosted = true;
        
        return Ok(status);
    }
    
    /// <summary>
    /// Undo a reshare of a status.
    /// </summary>
    /// <param name="id">The ID of the Status in the database.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Produces("application/json")]
    [OAuth("write:favourites")]
    [Route("{id}/unreblog")]
    public async Task<IActionResult> Unreblog(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var boost = await repo.FindBoostByIdAndAuthor(
            user,
            id);
        
        if (boost is null || !boost.VisibleByUser(user))
            return NotFound(new MastodonApiError("Record not found."));

        await postManagementService.UndoBoost(
            user,
            boost);

        var status = statusRenderer.RenderForPost(boost.Boosting!);
        status.Boosted = false;
        
        return Ok(status);
    }

    /// <summary>
    /// View who favourited a given status.
    /// </summary>
    /// <param name="id">The ID of the Status in the database.</param>
    /// <returns>An array of <see cref="Account"/> on success.</returns>
    [HttpGet]
    [Route("{id}/favourited_by")]
    [OAuth(manualScopeValidation: true)]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<Account>>> GetLikes(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()?
            .User;

        // TODO: I'd love if we could do it in one query.
        var post = await repo.FindById(id);
        if (post is null || !post.VisibleByUser(user))
            return NotFound(new MastodonApiError("Record not found."));
        
        // TODO: Link based pagination.
        var users = await repo.CreateCustomLikeQuery()
            .OrderByDescending(like => like.Id)
            .Where(like => like.PostId == id)
            .Include(like => like.LikingUser)
            .Select(like => like.LikingUser)
            .ToListAsync();

        return Ok(users.Select(
            u => accountRenderer.RenderAccountFrom(u)));
    }
}