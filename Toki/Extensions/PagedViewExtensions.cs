using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Objects;
using Toki.MastodonApi.Schemas.Params;

namespace Toki.Extensions;

/// <summary>
/// Extensions for the PagedView[TModel] class.
/// </summary>
public static class PagedViewExtensions
{
    /// <summary>
    /// Adds the Mastodon pagination parameters onto a paged view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <returns>The updated paged view.</returns>
    public static PagedView<TModel> WithMastodonParams<TModel>(
        this PagedView<TModel> view,
        PaginationParams paginationParams)
    where TModel : AbstractModel
    {
        if (paginationParams.MaxId is not null)
            view = view.Before(paginationParams.MaxId.Value);

        if (paginationParams.SinceId is not null)
            view = view.After(paginationParams.SinceId.Value);

        return view.Limit(paginationParams.Limit);
    }
    
    /// <summary>
    /// Gets the list of items and sets the mastodon link-based pagination parameters.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="ctx">The HTTP context.</param>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <returns>The list of <see cref="TModel"/></returns>
    public static async Task<List<TModel>> GetWithMastodonPagination<TModel>(
        this PagedView<TModel> view, HttpContext ctx)
    where TModel : AbstractModel
    {
        var list = await view.ToList();

        if (list.Count <= 0)
            return list;
        
        var fst = list.First();
        var lst = list.Last();

        var route = $"https://{ctx.Request.Host}/{ctx.Request.Path}";
            
        ctx.Response.Headers.Link = $"<{route}?max_id={lst.Id}>; rel=\"next\", <{route}?since_id={fst.Id}>; rel=\"prev\"";

        return list;
    }
}