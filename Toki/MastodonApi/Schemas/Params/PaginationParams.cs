using Microsoft.AspNetCore.Mvc;

namespace Toki.MastodonApi.Schemas.Params;

/// <summary>
/// Parameters for pagination.
/// </summary>
public record PaginationParams
{
    /// <summary>
    /// All results returned will be lesser than this ID. In effect, sets an upper bound on results.
    /// </summary>
    [BindProperty(Name = "max_id")]
    public Ulid? MaxId { get; init; }
    
    /// <summary>
    /// Returns results immediately newer than this ID. In effect, sets a cursor at this ID and paginates forward.
    /// </summary>
    [BindProperty(Name = "min_id")]
    public Ulid? MinId { get; init; }
    
    /// <summary>
    /// All results returned will be greater than this ID. In effect, sets a lower bound on results.
    /// </summary>
    [BindProperty(Name = "since_id")]
    public Ulid? SinceId { get; init; }

    /// <summary>
    /// Maximum number of results to return. 
    /// </summary>
    [BindProperty(Name = "limit")]
    public int Limit { get; init; } = 20;
}