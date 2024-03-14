using Toki.ActivityPub.Models.Enums;

namespace Toki.ActivityPub.Models;

/// <summary>
/// A post.
/// </summary>
public class Post : RemoteableModel
{
    /// <summary>
    /// The author of this post.
    /// </summary>
    public required User Author { get; init; }
    
    /// <summary>
    /// The id of the author.
    /// </summary>
    public required Ulid AuthorId { get; init; }
    
    /// <summary>
    /// The content of this post.
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// The parent of this post.
    /// </summary>
    public Post? Parent { get; set; }
    
    /// <summary>
    /// The id of the parent post.
    /// </summary>
    public Ulid? ParentId { get; set; }
    
    /// <summary>
    /// The id of the current thread context.
    /// </summary>
    public Guid? Context { get; set; }
    
    /// <summary>
    /// The post this one is boosting.
    /// </summary>
    public Post? Boosting { get; set; }
    
    /// <summary>
    /// The id of the post we're boosting.
    /// </summary>
    public Ulid? BoostingId { get; set; }
    
    /// <summary>
    /// Is this post sensitive?
    /// </summary>
    public bool Sensitive { get; set; }

    /// <summary>
    /// The content warning on this post.
    /// </summary>
    public string? ContentWarning { get; set; }
    
    /// <summary>
    /// The visibility of this post.
    /// </summary>
    public PostVisibility Visibility { get; set; }
    
    /// <summary>
    /// The like count of this post (DENORMALIZED).
    /// </summary>
    public int LikeCount { get; set; }
    
    /// <summary>
    /// The boost count of this post (DENORMALIZED).
    /// </summary>
    public int BoostCount { get; set; }
    
    /// <summary>
    /// The list of mentioned users (DENORMALIZED).
    /// </summary>
    public List<string>? Mentions { get; set; }
    
    /// <summary>
    /// The attachments this post has.
    /// </summary>
    public ICollection<PostAttachment>? Attachments { get; set; }
    
    /// <summary>
    /// The likes this post has.
    /// </summary>
    public ICollection<PostLike>? Likes { get; private set; }
}