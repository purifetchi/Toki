using Toki.ActivityPub.Models.Enums;

namespace Toki.ActivityPub.Models.DTO;

/// <summary>
/// A creation request for a post.
/// </summary>
public record PostCreationRequest
{
    /// <summary>
    /// The author.
    /// </summary>
    public required User Author { get; init; }
    
    /// <summary>
    /// The content of the post.
    /// </summary>
    public required string Content { get; init; }
    
    /// <summary>
    /// The visibility of this post.
    /// </summary>
    public required PostVisibility Visibility { get; init; }
    
    /// <summary>
    /// Is the post sensitive.
    /// </summary>
    public bool IsSensitive { get; init; }
    
    /// <summary>
    /// The content warning.
    /// </summary>
    public string? ContentWarning { get; init; }
    
    /// <summary>
    /// The post we're replying to.
    /// </summary>
    public Post? InReplyTo { get; init; }
    
    /// <summary>
    /// The media that are going to be attached to the post.
    /// </summary>
    public IList<PostAttachment>? Media { get; init; }
}