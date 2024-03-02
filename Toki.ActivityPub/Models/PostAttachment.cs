namespace Toki.ActivityPub.Models;

/// <summary>
/// An attachment in a post.
/// </summary>
public class PostAttachment : AbstractModel
{
    /// <summary>
    /// The parent of this attachment.
    /// </summary>
    public required Post Parent { get; init; }
    
    /// <summary>
    /// The id of the parent.
    /// </summary>
    public required Guid ParentId { get; init; }
    
    /// <summary>
    /// The link to the attachment.
    /// </summary>
    public required string Url { get; init; }
    
    /// <summary>
    /// The description of the attachment.
    /// </summary>
    public string? Description { get; set; }
}