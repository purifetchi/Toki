namespace Toki.ActivityPub.Configuration;

/// <summary>
/// Configuration section for various limits.
/// </summary>
public class LimitsConfiguration
{
    /// <summary>
    /// The character limit for this instance.
    /// </summary>
    public int MaxPostCharacterLimit { get; set; } = 1024;

    /// <summary>
    /// The maximum amount of characters in a user's bio.
    /// </summary>
    public int MaxBioCharacterLimit { get; set; } = 1024;

    /// <summary>
    /// The maximum amount of pinned posts.
    /// </summary>
    public int MaxPinnedPosts { get; set; } = 10;

    /// <summary>
    /// The max attachment count for this instance.
    /// </summary>
    public int MaxAttachmentCount { get; set; } = 4;
}