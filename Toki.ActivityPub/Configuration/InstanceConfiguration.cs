namespace Toki.ActivityPub.Configuration;

/// <summary>
/// The configuration for this instance.
/// </summary>
public class InstanceConfiguration
{
    /// <summary>
    /// The domain of this instance.
    /// </summary>
    public required string Domain { get; init; }
    
    /// <summary>
    /// The information about this instance. 
    /// </summary>
    public string? Info { get; set; }
    
    /// <summary>
    /// The contact e-mail.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Is signed fetching enabled?
    /// </summary>
    public bool SignedFetch { get; set; } = false;
}