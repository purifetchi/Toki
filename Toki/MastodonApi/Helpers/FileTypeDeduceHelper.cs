using Toki.ActivityPub.Models;

namespace Toki.MastodonApi.Helpers;

/// <summary>
/// A helper class for deducing the Mastodon attachment file type.
/// </summary>
public static class FileTypeDeduceHelper
{
    /// <summary>
    /// Gets the file type for an attachment.
    /// </summary>
    /// <param name="attachment">The attachment.</param>
    /// <returns>The file type.</returns>
    public static string GetFileType(PostAttachment attachment)
    {
        // Try to get the type from the MIME first.
        var type = attachment.Mime?[..attachment.Mime.IndexOf('/')];
        if (type is "video" or "image" or "audio")
            return type;
        
        // If we've gotten nothing, or, god forbid, "application" try to deduce from the extension.
        var ext = Path.GetExtension(attachment.Url)
            .ToLower();

        return ext switch
        {
            ".jpeg" or ".jpg" or ".png" or ".gif" or ".svg" or ".webp" => "image",
            ".webm" or ".mp4" or ".ogv" => "video",
            ".ogg" or ".mp3" or ".flac" or ".opus" => "audio",
            _ => "unknown"
        };
    }
}