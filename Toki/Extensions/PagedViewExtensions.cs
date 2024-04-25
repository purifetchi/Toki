using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Objects;

namespace Toki.Extensions;

/// <summary>
/// Extensions for the PagedView[TModel] class.
/// </summary>
public static class PagedViewExtensions
{
    /// <summary>
    /// Gets the list of items and sets the mastodon link-based pagination parameters.
    /// </summary>
    /// <param name="view"></param>
    /// <param name="ctx"></param>
    /// <typeparam name="TModel"></typeparam>
    /// <returns></returns>
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