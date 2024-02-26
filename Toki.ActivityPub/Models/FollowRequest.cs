namespace Toki.ActivityPub.Models;

/// <summary>
/// A follow request.
/// </summary>
public class FollowRequest : RemoteableModel
{
    /// <summary>
    /// From who does this follow request originate.
    /// </summary>
    public required User From { get; init; }

    /// <summary>
    /// The ID of the origin user.
    /// </summary>
    public required Guid FromId { get; init; }
    
    /// <summary>
    /// To who is the follow request directed.
    /// </summary>
    public required User To { get; init; }
    
    /// <summary>
    /// The id of the destination user.
    /// </summary>
    public required Guid ToId { get; init; }
}