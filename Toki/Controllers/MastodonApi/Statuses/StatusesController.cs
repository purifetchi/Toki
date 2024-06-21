using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Extensions;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.DTO;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Posts;
using Toki.ActivityPub.Users;
using Toki.Binding;
using Toki.Extensions;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Errors;
using Toki.MastodonApi.Schemas.Objects;
using Toki.MastodonApi.Schemas.Params;
using Toki.MastodonApi.Schemas.Requests.Statuses;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;
using Toki.Services.Timelines;

namespace Toki.Controllers.MastodonApi.Statuses;

/// <summary>
/// The controller for the "/api/v1/statuses" endpoint.
/// </summary>
[ApiController]
[Route("/api/v1/statuses")]
[EnableCors("MastodonAPI")]
public class StatusesController(
    PostManagementService postManagementService,
    FollowRepository followRepository,
    TimelineBuilder timelineBuilder,
    PostRepository repo,
    StatusRenderer statusRenderer,
    AccountRenderer accountRenderer) : ControllerBase
{
    /// <summary>
    /// Returns whether a post is visible by a user.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <param name="user">The user.</param>
    /// <returns>Whether the post is visible by them.</returns>
    private async Task<bool> PostVisibleByUser(
        Post post,
        User? user)
    {
        if (user?.Id == post.AuthorId)
            return true;
        
        return post.Visibility switch
        {
            PostVisibility.Public or PostVisibility.Unlisted => true,
            PostVisibility.Followers when 
                user != null => await followRepository.FollowRelationExistsBetween(user, post.Author),
            PostVisibility.Direct when
                user != null && post.Mentions?.Contains(user.Id.ToString()) == true => true,
        
            _ => false
        };
    }
    
    /// <summary>
    /// Posts a request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Consumes("application/json", "application/x-www-form-urlencoded", "multipart/form-data")]
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
            await statusRenderer.RenderStatusForUser(user, post));
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
        if (post is null || !await PostVisibleByUser(post, user))
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
        if (post is null || !await PostVisibleByUser(post, user))
            return NotFound(new MastodonApiError("Record not found."));

        if (post.Context is null)
        {
            return Ok(new Context
            {
                Parents = [await statusRenderer.RenderStatusForUser(user, post)]
            });
        }
        
        var repliesInContext = await timelineBuilder
            .NewWithoutOrdering()
            .Filter(p => p.Context == post.Context)
            .GetTimeline();
        
        // TODO: This sucks and it sucks horribly, I have no clue right now how to make it better...
        //       Iceshrimp seems to make use of a database function, from what I can tell, although
        //       I couldn't find any code related to it.
        var parents = new List<Post>();
        
        // Traverse the tree upwards.
        var temp = post;
        while (temp?.ParentId is not null)
        {
            temp = repliesInContext.FirstOrDefault(p => p.Id == temp.ParentId);
            if (temp is not null && await PostVisibleByUser(temp, user))
                parents.Insert(0, temp);
        }

        var children = new List<Post>();
        var queue = new Queue<Ulid>();
        queue.Enqueue(post.Id);

        // Traverse the tree downwards.
        while (queue.TryDequeue(out var postId))
        {
            foreach (var reply in repliesInContext
                         .Where(p => p.ParentId == postId))
            {
                queue.Enqueue(reply.Id);
                if (await PostVisibleByUser(reply, user))
                    children.Add(reply);
            }
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
        if (post is null || !await PostVisibleByUser(post, user))
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
        if (post is null || !await PostVisibleByUser(post, user))
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
        if (post is null || !await PostVisibleByUser(post, user))
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
        
        if (boost is null || !await PostVisibleByUser(boost, user))
            return NotFound(new MastodonApiError("Record not found."));

        await postManagementService.UndoBoost(
            user,
            boost);

        var status = statusRenderer.RenderForPost(boost.Boosting!);
        status.Boosted = false;
        
        return Ok(status);
    }
    
    /// <summary>
    /// Privately bookmark a status.
    /// </summary>
    /// <param name="id">The id of the post.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Produces("application/json")]
    [OAuth("write:bookmarks")]
    [Route("{id}/bookmark")]
    public async Task<IActionResult> Bookmark(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var post = await repo.FindById(id);
        if (post is null || !await PostVisibleByUser(post, user))
            return NotFound(new MastodonApiError("Record not found."));

        await postManagementService.Bookmark(
            user,
            post);

        var status = statusRenderer.RenderForPost(post);
        status.Bookmarked = true;
        
        return Ok(status);
    }
    
    /// <summary>
    /// Remove a status from your private bookmarks.
    /// </summary>
    /// <param name="id">The id of the post.</param>
    /// <returns>A <see cref="Toki.MastodonApi.Schemas.Objects.Status"/> on success.</returns>
    [HttpPost]
    [Produces("application/json")]
    [OAuth("write:bookmarks")]
    [Route("{id}/unbookmark")]
    public async Task<IActionResult> Unbookmark(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var post = await repo.FindById(id);
        if (post is null || !await PostVisibleByUser(post, user))
            return NotFound(new MastodonApiError("Record not found."));
        
        await postManagementService.UndoBookmark(
            user,
            post);

        var status = statusRenderer.RenderForPost(post);
        status.Bookmarked = false;
        
        return Ok(status);
    }

    /// <summary>
    /// View who favourited a given status.
    /// </summary>
    /// <param name="id">The ID of the Status in the database.</param>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <returns>An array of <see cref="Account"/> on success.</returns>
    [HttpGet]
    [Route("{id}/favourited_by")]
    [OAuth(manualScopeValidation: true)]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<Account>>> GetLikes(
        [FromRoute] Ulid id,
        [FromQuery] PaginationParams paginationParams)
    {
        var user = HttpContext.GetOAuthToken()?
            .User;

        // TODO: I'd love if we could do it in one query.
        var post = await repo.FindById(id);
        if (post is null || !await PostVisibleByUser(post, user))
            return NotFound(new MastodonApiError("Record not found."));
        
        var likesView = repo.GetLikesForPost(post);
        var users = await likesView
            .WithMastodonParams(paginationParams)
            .Project<User>(l => l.LikingUser)
            .GetWithMastodonPagination(HttpContext);

        return Ok(users.Select(
            u => accountRenderer.RenderAccountFrom(u)));
    }
    
    /// <summary>
    /// View who boosted a given status.
    /// </summary>
    /// <param name="id">The ID of the Status in the database.</param>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <returns>An array of <see cref="Account"/> on success.</returns>
    [HttpGet]
    [Route("{id}/reblogged_by")]
    [OAuth(manualScopeValidation: true)]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<Account>>> GetBoosts(
        [FromRoute] Ulid id,
        [FromQuery] PaginationParams paginationParams)
    {
        var user = HttpContext.GetOAuthToken()?
            .User;

        // TODO: I'd love if we could do it in one query.
        var post = await repo.FindById(id);
        if (post is null || !await PostVisibleByUser(post, user))
            return NotFound(new MastodonApiError("Record not found."));
        
        var boostsView = repo.GetBoostsForPost(post);
        var users = await boostsView
            .WithMastodonParams(paginationParams)
            .Project<User>(b => b.Author)
            .GetWithMastodonPagination(HttpContext);

        return Ok(users.Select(
            u => accountRenderer.RenderAccountFrom(u)));
    }

    /// <summary>
    /// Feature one of your own public statuses at the top of your profile.
    /// </summary>
    /// <param name="id">The local ID of the Status in the database.</param>
    /// <returns>A <see cref="Status"/> on success.</returns>
    [HttpPost]
    [Route("{id}/pin")]
    [OAuth("write:accounts")]
    [Produces("application/json")]
    public async Task<ActionResult<Status>> PinStatus(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var post = await repo.FindById(id);
        if (post is null || !await PostVisibleByUser(post, user))
            return NotFound(new MastodonApiError("Record not found."));

        if (post.Author != user)
            return UnprocessableEntity(new MastodonApiError("Validation failed: You cannot pin someone else's post."));

        await postManagementService.Pin(post);

        var status = await statusRenderer.RenderStatusForUser(user, post);
        status.Pinned = true;

        return Ok(status);
    }
    
    /// <summary>
    /// Unfeature a status from the top of your profile.
    /// </summary>
    /// <param name="id">The local ID of the Status in the database.</param>
    /// <returns>A <see cref="Status"/> on success.</returns>
    [HttpPost]
    [Route("{id}/unpin")]
    [OAuth("write:accounts")]
    [Produces("application/json")]
    public async Task<ActionResult<Status>> UnpinStatus(
        [FromRoute] Ulid id)
    {
        var user = HttpContext.GetOAuthToken()!
            .User;

        var post = await repo.FindById(id);
        if (post is null || !await PostVisibleByUser(post, user))
            return NotFound(new MastodonApiError("Record not found."));

        if (post.Author != user)
            return UnprocessableEntity(new MastodonApiError("Validation failed: You cannot pin someone else's post."));

        await postManagementService.Unpin(post);

        var status = await statusRenderer.RenderStatusForUser(user, post);
        status.Pinned = false;

        return Ok(status);
    }
}