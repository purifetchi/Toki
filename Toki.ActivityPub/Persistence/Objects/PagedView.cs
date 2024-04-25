using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;

namespace Toki.ActivityPub.Persistence.Objects;

/// <summary>
/// A paged view into a database.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public class PagedView<TModel>(IQueryable<TModel> query)
    where TModel : AbstractModel
{
    /// <summary>
    /// Paginates the view showing items that happened before some ID.
    /// </summary>
    /// <param name="id">Before what id should we paginate.</param>
    /// <returns>This.</returns>
    public PagedView<TModel> Before(
        Ulid id)
    {
        query = query.Where(m => m.Id.CompareTo(id) == -1);
        return this;
    }

    /// <summary>
    /// Paginates the view showing items that happened after some ID.
    /// </summary>
    /// <param name="id">After what id should we paginate.</param>
    /// <returns>This.</returns>
    public PagedView<TModel> After(
        Ulid id)
    {
        query = query.Where(m => m.Id.CompareTo(id) == 1);
        return this;
    }

    /// <summary>
    /// Limits the amount of objects we can fetch.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <returns>This.</returns>
    public PagedView<TModel> Limit(
        int count)
    {
        query = query.Take(count);
        return this;
    }

    /// <summary>
    /// Projects this paged view into another type.
    /// </summary>
    /// <param name="func">The projection function.</param>
    /// <typeparam name="TOther">The other type.</typeparam>
    /// <returns>A paged view into <see cref="TOther"/></returns>
    public PagedView<TOther> Project<TOther>(
        Expression<Func<TModel, TOther>> func)
        where TOther : AbstractModel
    {
        return new PagedView<TOther>(
            query.Select(func));
    }
    
    /// <summary>
    /// Gets the view as a list.
    /// </summary>
    /// <returns>The list.</returns>
    public async Task<List<TModel>> ToList()
    {
        return await query
            .ToListAsync();
    }
}