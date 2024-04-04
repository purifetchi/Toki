namespace Toki.Services.Usage.Models;

/// <summary>
/// The usage statistics for this Toki instance.
/// </summary>
/// <param name="ActiveThisMonth">The amount of users active this month.</param>
/// <param name="ActiveThisHalfYear">The amount of users active this half year.</param>
/// <param name="LocalPosts">The amount of local posts.</param>
/// <param name="UserCount">The count of users on this instance.</param>
/// <param name="PeerCount">The count of domains that this instance has discovered.</param>
public record UsageStatistics(
    int ActiveThisMonth,
    int ActiveThisHalfYear,
    int LocalPosts,
    int UserCount,
    int PeerCount);