namespace Toki.ActivityPub.Models.Posts;

/// <summary>
/// A mention in a post.
/// </summary>
public class PostMention
{
    /// <summary>
    /// The ID of the user.
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// The url of the user.
    /// </summary>
    public required string Url { get; set; }
    
    /// <summary>
    /// The handle of the user.
    /// </summary>
    public required string Handle { get; set; }
}