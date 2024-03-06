using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.Configuration;

namespace Toki.Services.Drive;

/// <summary>
/// A drive service for uploading media content.
/// </summary>
public class DriveService(
    IOptions<UploadConfiguration> opts,
    IOptions<InstanceConfiguration> instanceOpts)
{
    /// <summary>
    /// The list of accepted MIME types.
    /// </summary>
    public static IReadOnlyList<string> AcceptedMIMEs { get; } = 
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/heic",
        "image/heif",
        "image/webp",
        "image/avif",
        "video/webm",
        "video/mp4",
        "video/quicktime",
        "video/ogg",
        "audio/wave",
        "audio/wav",
        "audio/x-wav",
        "audio/x-pn-wave",
        "audio/vnd.wave",
        "audio/ogg",
        "audio/vorbis",
        "audio/mpeg",
        "audio/mp3",
        "audio/webm",
        "audio/flac",
        "audio/aac",
        "audio/m4a",
        "audio/x-m4a",
        "audio/mp4",
        "audio/3gpp",
        "video/x-ms-asf"
    ];
    
    /// <summary>
    /// Stores a file from the form upload. 
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>The url to the file, if the upload was successful.</returns>
    public async Task<string?> Store(
        IFormFile file)
    {
        if (!AcceptedMIMEs.Contains(file.ContentType))
            return null;

        if (file.Length > opts.Value.MaxFileSize)
            return null;

        if (opts.Value.UploadFolderPath is null)
            return null;

        var filename = Path.GetFileName(file.FileName);
        var stream = file.OpenReadStream();
        var pos = stream.Position;
        
        var hash = Convert.ToHexString(
            await MD5.HashDataAsync(stream))
            .ToLowerInvariant();

        Directory.CreateDirectory(
            opts.Value.UploadFolderPath + $"/{hash}");

        await using var fs = new FileStream($"{opts.Value.UploadFolderPath}/{hash}/{filename}", 
            FileMode.Create,
            FileAccess.Write, 
            FileShare.None);

        stream.Position = pos;

        await stream.CopyToAsync(fs);
        
        return $"https://{instanceOpts.Value.Domain}/media/{hash}/{file.FileName}";
    }
}