using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.Extensions;

namespace Toki.Services.Timelines;

/// <summary>
/// A helper class for building timelines.
/// </summary>
public class TimelineBuilder(
    PostRepository postRepo)
{
    /// <summary>
    /// The query we're operating on.
    /// </summary>
    private IQueryable<Post> _query = postRepo.CreateCustomQuery()
        .AddMastodonRenderNecessities()
        .OrderByDescending(post => post.ReceivedAt);

    /// <summary>
    /// Since what should we paginate?
    /// </summary>
    private Guid? _since = null;

    /// <summary>
    /// The count to get.
    /// </summary>
    private int _count = 0;

    /// <summary>
    /// Paginates this query.
    /// </summary>
    /// <param name="since">What ID to start at?</param>
    /// <param name="count">The count to fetch.</param>
    /// <returns>Ourselves.</returns>
    public TimelineBuilder Paginate(Guid since, int count)
    {
        const int maxResults = 40;
        
        _since = since;
        _count = count > maxResults ? maxResults : count;

        return this;
    }

    /// <summary>
    /// Sets this timeline to be viewed as it was by someone.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Ourselves.</returns>
    public TimelineBuilder ViewAs(User? user)
    {
        if (user is not null)
        {
            _query = _query
                .Include(post => post.Author)
                    .ThenInclude(author => author.FollowerRelations)
                .Where(post => post.Visibility == PostVisibility.Public ||
                               post.Visibility == PostVisibility.Unlisted ||
                               (post.Visibility == PostVisibility.Followers && (post.AuthorId == user.Id || 
                                (post.Author.FollowerRelations != null &&
                                 post.Author.FollowerRelations.Any(fr => fr.FollowerId == user.Id)))));
            
            // TODO: Direct messages.
        }
        else
        {
            _query = _query.Where(post => post.Visibility == PostVisibility.Public ||
                                          post.Visibility == PostVisibility.Unlisted);
        }

        return this;
    }

    /// <summary>
    /// Filters based on a predicate.
    /// </summary>
    /// <param name="expr">The expression.</param>
    /// <returns>Ourselves.</returns>
    public TimelineBuilder Filter(Expression<Func<Post, bool>> expr)
    {
        _query = _query.Where(expr);
        return this;
    }

    /// <summary>
    /// Gets the resulting timeline.
    /// </summary>
    /// <returns>The timeline.</returns>
    public async Task<IEnumerable<Post>> GetTimeline()
    {
        if (_since is null)
            return await _query.ToListAsync();
        
        var lastDate = await postRepo.CreateCustomQuery()
            .Where(post => post.Id == _since)
            .Select(post => post.ReceivedAt)
            .FirstOrDefaultAsync();
            
        return await _query
            .Where(post => post.ReceivedAt < lastDate)
            .Take(_count)
            .ToListAsync();
    }
}