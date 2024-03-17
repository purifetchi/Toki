using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Models.DTO;

/// <summary>
/// The result of the content formatting pipeline.
/// </summary>
/// <param name="Formatted">The formatted content.</param>
/// <param name="Mentions">The mentions inside.</param>
public record ContentFormattingResult(
    string Formatted,
    IReadOnlyList<User> Mentions);
// TODO: Emojis