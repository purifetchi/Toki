namespace Toki.ActivityPub.Models;

/// <summary>
/// A custom emoji.
/// </summary>
public class Emoji : AbstractModel
{
    /// <summary>
    /// The URL of the emoji.
    /// </summary>
    public required string RemoteUrl { get; init; }
    
    /// <summary>
    /// The shortcode of the emoji.
    /// </summary>
    public required string Shortcode { get; init; }
    
    /// <summary>
    /// The category of the emoji.
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// The parent instance this emoji belongs to.
    /// </summary>
    public RemoteInstance? ParentInstance { get; set; }
    
    /// <summary>
    /// The ID of the parent instance.
    /// </summary>
    public Ulid? ParentInstanceId { get; set; }
}