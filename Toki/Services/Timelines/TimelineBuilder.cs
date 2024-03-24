using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.Extensions;
using Toki.MastodonApi.Schemas.Params;

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
        .OrderByDescending(post => post.Id);

    /// <summary>
    /// The pagination params.
    /// </summary>
    private PaginationParams? _paginationParams = null;

    /// <summary>
    /// The count to get.
    /// </summary>
    private int _count = 0;

    /// <summary>
    /// Paginates this query.
    /// </summary>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <returns>Ourselves.</returns>
    public TimelineBuilder Paginate(
        PaginationParams paginationParams)
    {
        const int maxResults = 40;
        
        _paginationParams = paginationParams;
        _count = paginationParams.Limit > maxResults ? 
            maxResults : 
            paginationParams.Limit;

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
    public async Task<IList<Post>> GetTimeline()
    {
        if (_paginationParams is null)
            return await _query.ToListAsync();

        // TODO: Checking out the EXPLAIN output of this, we could save A LOT of processing
        //       if not for the fact that EF Core puts some really weird stuff in the ORDER BY
        //       clause. I think we can somehow optimize it later down the line.
        if (_paginationParams.MaxId is not null)
        {
            var q = _query
                .Where(post => post.Id.CompareTo(_paginationParams.MaxId.Value) == -1)
                .Take(_count);
            
            return await q.ToListAsync();
        }
        
        return await _query
            .Take(_count)
            .ToListAsync();
    }
}