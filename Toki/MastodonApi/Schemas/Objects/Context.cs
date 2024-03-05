using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// Represents the tree around a given status. Used for reconstructing threads of statuses
/// </summary>
public record Context
{
    /// <summary>
    /// Parents in the thread.
    /// </summary>
    [JsonPropertyName("ancestors")]
    public IReadOnlyList<Status>? Parents { get; init; } = [];

    /// <summary>
    /// Children in the thread.
    /// </summary>
    [JsonPropertyName("descendants")]
    public IReadOnlyList<Status>? Children { get; init; } = [];
}