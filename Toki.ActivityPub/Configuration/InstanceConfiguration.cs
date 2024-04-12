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
    /// The limits configuration.
    /// </summary>
    public LimitsConfiguration Limits { get; set; } = new LimitsConfiguration();

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

    /// <summary>
    /// Does this instance support OAuth2 password login?
    /// </summary>
    public bool SupportPasswordGrantType { get; set; } = true;

    /// <summary>
    /// Should we fetch the pinned posts for an actor that we're fetching for the first time?
    /// </summary>
    public bool FetchPinnedPostsOnFirstRemoteProfileFetch { get; set; } = true;

    /// <summary>
    /// The UserAgent string of Toki.
    /// </summary>
    public string UserAgent =>
        $"{Software.SoftwareName}/{Software.SoftwareVersion} ({Domain}; <{ContactEmail}>)";
}