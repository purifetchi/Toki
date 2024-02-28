namespace Toki.ActivityPub.Models.Enums;

/// <summary>
/// The visibility of this post.
/// </summary>
public enum PostVisibility
{
    /// <summary>
    /// This post should be displayed on the whole known network
    /// </summary>
    Public,
    
    /// <summary>
    /// This post should be visible on the timelines of followers and on local timelines
    /// </summary>
    Unlisted,
    
    /// <summary>
    /// This post should be visible to followers of this user.
    /// </summary>
    Followers,
    
    /// <summary>
    /// This post should only be visible to mentioned users.
    /// </summary>
    Direct
}