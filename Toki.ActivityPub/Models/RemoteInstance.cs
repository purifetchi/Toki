namespace Toki.ActivityPub.Models;

/// <summary>
/// A remote instance.
/// </summary>
public class RemoteInstance : AbstractModel
{
    /// <summary>
    /// The domain of the instance.
    /// </summary>
    public required string Domain { get; init; }
    
    /// <summary>
    /// The shared inbox of this instance.
    /// </summary>
    public string? SharedInbox { get; set; }
    
    /// <summary>
    /// The name of the instance.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// The description of the instance.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The software running on this instance.
    /// </summary>
    public string? Software { get; set; }
}