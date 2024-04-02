using Microsoft.AspNetCore.Mvc;
using Toki.Binding;

namespace Toki.MastodonApi.Schemas.Requests.Accounts;

/// <summary>
/// Filters for the statuses get request.
/// </summary>
public record StatusesFilters
{
    /// <summary>
    /// Filter for pinned statuses only. Defaults to false, which includes all statuses.
    /// Pinned statuses do not receive special priority in the order of the returned results.
    /// </summary>
    [BindProperty(Name = "pinned", BinderType = typeof(MultipleBooleanModelBinder))]
    public bool Pinned { get; init; } = false;
    
    /// <summary>
    /// Filter out statuses without attachments.
    /// </summary>
    [BindProperty(Name = "only_media", BinderType = typeof(MultipleBooleanModelBinder))]
    public bool OnlyMedia { get; init; } = false;

    /// <summary>
    /// Filter out statuses in reply to a different account.
    /// </summary>
    [BindProperty(Name = "exclude_replies", BinderType = typeof(MultipleBooleanModelBinder))]
    public bool ExcludeReplies { get; init; } = false;

    /// <summary>
    /// Filter out boosts from the response.
    /// </summary>
    [BindProperty(Name = "exclude_reblogs", BinderType = typeof(MultipleBooleanModelBinder))]
    public bool ExcludeBoosts { get; init; } = false;
}