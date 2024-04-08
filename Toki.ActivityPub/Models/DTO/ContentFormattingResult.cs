namespace Toki.ActivityPub.Models.DTO;

/// <summary>
/// The result of the content formatting pipeline.
/// </summary>
/// <param name="Formatted">The formatted content.</param>
/// <param name="Mentions">The mentions inside.</param>
/// <param name="Hashtags">The hashtags.</param>
/// <param name="Emojis">The emojis.</param>
public record ContentFormattingResult(
    string Formatted,
    IReadOnlyList<User> Mentions,
    List<string> Hashtags,
    List<string> Emojis);