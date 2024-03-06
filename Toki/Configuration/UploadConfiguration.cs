namespace Toki.Configuration;

/// <summary>
/// Configuration for file uploads.
/// </summary>
public class UploadConfiguration
{
    /// <summary>
    /// The path to the upload folder.
    /// </summary>
    public string? UploadFolderPath { get; set; }
    
    /// <summary>
    /// The maximum file size.
    /// </summary>
    public long MaxFileSize { get; set; }
}