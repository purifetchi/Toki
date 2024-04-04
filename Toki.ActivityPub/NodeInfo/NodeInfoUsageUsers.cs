using System.Text.Json.Serialization;

namespace Toki.ActivityPub.NodeInfo;

/// <summary>
/// The usage statistics of the users.
/// </summary>
public record NodeInfoUsageUsers
{
    /// <summary>
    /// The total amount of users.
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; init; }
    
    /// <summary>
    /// The amount of users active this past half a year.
    /// </summary>
    [JsonPropertyName("activeHalfyear")]
    public int ActiveHalfYear { get; init; }
    
    /// <summary>
    /// The amount of users active this past month.
    /// </summary>
    [JsonPropertyName("activeMonth")]
    public int ActiveMonth { get; init; }
}