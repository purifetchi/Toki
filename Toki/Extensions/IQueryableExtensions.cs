using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;

namespace Toki.Extensions;

/// <summary>
/// Extensions for various <see cref="IQueryable{T}"/> interfaces.
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Adds the required fields for rendering of mastodon statuses to the query.
    /// </summary>
    /// <param name="q">The query.</param>
    /// <returns>The query with all the joined fields.</returns>
    public static IQueryable<Post> AddMastodonRenderNecessities(this IQueryable<Post> q) => q
        .Include(post => post.Author)
        .Include(post => post.Parent)
        .Include(post => post.Attachments)
        .Include(post => post.Boosting)
        .ThenInclude(boost => boost!.Author)
        .Include(post => post.Boosting)
        .ThenInclude(boost => boost!.Attachments);
}