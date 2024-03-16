namespace Toki.ActivityPub.Models;

/// <summary>
/// A base model that can be remote.
/// </summary>
public abstract class RemoteableModel : AbstractModel
{
    /// <summary>
    /// Constructs a new remoteable model.
    /// </summary>
    public RemoteableModel()
    {
        CreatedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// When was this object created?
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// The remote ID of this object.
    /// </summary>
    public string? RemoteId { get; set; }
}