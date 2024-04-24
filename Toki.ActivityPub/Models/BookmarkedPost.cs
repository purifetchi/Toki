namespace Toki.ActivityPub.Models;

/// <summary>
/// A bookmarked post.
/// </summary>
public class BookmarkedPost : AbstractModel
{
    /// <summary>
    /// The ID of the post.
    /// </summary>
    public required Ulid PostId { get; init; }
    
    /// <summary>
    /// The post itself.
    /// </summary>
    public required Post Post { get; init; }
    
    /// <summary>
    /// The ID of the user who has bookmarked the post.
    /// </summary>
    public required Ulid UserId { get; init; }
    
    /// <summary>
    /// The user.
    /// </summary>
    public required User User { get; init; }
}