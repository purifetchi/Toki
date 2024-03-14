namespace Toki.ActivityPub.Models;

/// <summary>
/// A post like.
/// </summary>
public class PostLike : AbstractModel
{
    /// <summary>
    /// The post.
    /// </summary>
    public required Post Post { get; init; }
    
    /// <summary>
    /// The id of the post.
    /// </summary>
    public required Ulid PostId { get; init; }
    
    /// <summary>
    /// The user who liked this post.
    /// </summary>
    public required User LikingUser { get; init; }
    
    /// <summary>
    /// The id of the user.
    /// </summary>
    public required Ulid LikingUserId { get; init; }
}