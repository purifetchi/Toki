namespace Toki.Configuration;

/// <summary>
/// Configuration for the front-end.
/// </summary>
public class FrontendConfiguration
{
    /// <summary>
    /// Should the front-end be enabled?
    /// </summary>
    public bool Enabled { get; init; }
    
    /// <summary>
    /// The single-page app filename.
    /// </summary>
    public string? SpaFilename { get; init; }
    
    /// <summary>
    /// The path to this instance's thumbnail.
    /// </summary>
    public string? ThumbnailPath { get; init; }
}