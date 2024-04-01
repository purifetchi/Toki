namespace Toki.ActivityPub.Models;

/// <summary>
/// A pinned post.
/// </summary>
public class PinnedPost : AbstractModel
{
    /// <summary>
    /// The user who's pinned the post.
    /// </summary>
    public required User User { get; init; }
    
    /// <summary>
    /// The ID of the user.
    /// </summary>
    public required Ulid UserId { get; init; }
    
    /// <summary>
    /// The pinned post.
    /// </summary>
    public required Post Post { get; init; }
    
    /// <summary>
    /// The ID of the post.
    /// </summary>
    public required Ulid PostId { get; init; }
}