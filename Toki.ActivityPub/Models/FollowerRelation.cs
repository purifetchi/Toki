namespace Toki.ActivityPub.Models;

/// <summary>
/// A follower relation.
/// </summary>
public class FollowerRelation : AbstractModel
{
    /// <summary>
    /// The follower user.
    /// </summary>
    public required User Follower { get; init; }
    
    /// <summary>
    /// The id of the follower.
    /// </summary>
    public required Guid FollowerId { get; init; }
    
    /// <summary>
    /// The followee user.
    /// </summary>
    public required User Followee { get; init; }
    
    /// <summary>
    /// The ID of the followee.
    /// </summary>
    public required Guid FolloweeId { get; init; }
}