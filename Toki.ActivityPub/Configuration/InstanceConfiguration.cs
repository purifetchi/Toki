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
    /// The software configuration.
    /// </summary>
    public SoftwareConfiguration Software { get; set; } = new SoftwareConfiguration();

    /// <summary>
    /// Does this instance have open registrations?
    /// </summary>
    public bool OpenRegistrations { get; set; } = false;

    /// <summary>
    /// The name of the instance.
    /// </summary>
    public string Name { get; set; } = "Toki Server";

    /// <summary>
    /// The information about this instance. 
    /// </summary>
    public string Info { get; set; } = "toki!";
    
    /// <summary>
    /// The contact e-mail.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Is signed fetching enabled?
    /// </summary>
    public bool SignedFetch { get; set; } = false;
}