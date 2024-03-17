using Microsoft.AspNetCore.Mvc;

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
    [BindProperty(Name = "pinned")]
    public bool Pinned { get; init; } = false;
    
    /// <summary>
    /// Filter out statuses without attachments.
    /// </summary>
    [BindProperty(Name = "only_media")]
    public bool OnlyMedia { get; init; } = false;

    /// <summary>
    /// Filter out statuses in reply to a different account.
    /// </summary>
    [BindProperty(Name = "exclude_replies")]
    public bool ExcludeReplies { get; init; } = false;

    /// <summary>
    /// Filter out boosts from the response.
    /// </summary>
    [BindProperty(Name = "exclude_reblogs")]
    public bool ExcludeBoosts { get; init; } = false;
}