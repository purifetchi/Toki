using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Toki.MastodonApi.Schemas.Requests.Search;

/// <summary>
/// The search request.
/// </summary>
public record SearchRequest
{
    /// <summary>
    /// The search query.
    /// </summary>
    [JsonPropertyName("q")]
    [BindProperty(Name = "q")]
    public required string Query { get; init; }

    /// <summary>
    /// Attempt WebFinger lookup? 
    /// </summary>
    [JsonPropertyName("resolve")]
    [BindProperty(Name = "resolve")]
    public bool Resolve { get; init; } = false;
    
    // TODO: type, following, account_id, exclude_unreviewed, max_id, min_id, limit, offset
}