namespace Toki.ActivityPub.Configuration;

/// <summary>
/// The configuration of the software.
/// </summary>
public class SoftwareConfiguration
{
    /// <summary>
    /// The reported name of the software. You can change it, I don't mind :)
    /// </summary>
    public string SoftwareName { get; set; } = "Toki";

    /// <summary>
    /// The software website.
    /// </summary>
    public string? SoftwareWebsite { get; set; } = "https://github.com/purifetchi/Toki";
    
    /// <summary>
    /// The software repository.
    /// </summary>
    public string? SoftwareRepository { get; set; } = "https://github.com/purifetchi/Toki";
    
    /// <summary>
    /// The version of the software. If null, it will default to the git revision if it's available.
    /// </summary>
    public string? SoftwareVersion { get; set; }
}