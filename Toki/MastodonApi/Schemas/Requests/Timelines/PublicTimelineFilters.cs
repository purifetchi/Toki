using Microsoft.AspNetCore.Mvc;
using Toki.Binding;

namespace Toki.MastodonApi.Schemas.Requests.Timelines;

/// <summary>
/// Filters for the public timeline query.
/// </summary>
public record PublicTimelineFilters
{
    /// <summary>
    /// Should we only fetch media? 
    /// </summary>
    [BindProperty(Name = "only_media", BinderType = typeof(MultipleBooleanModelBinder))]
    public bool OnlyMedia { get; init; }
    
    /// <summary>
    /// Should we only fetch local posts?
    /// </summary>
    [BindProperty(Name = "local", BinderType = typeof(MultipleBooleanModelBinder))]
    public bool Local { get; init; }
    
    /// <summary>
    /// Should we only fetch remote posts?
    /// </summary>
    [BindProperty(Name = "remote", BinderType = typeof(MultipleBooleanModelBinder))]
    public bool Remote { get; init; }
}