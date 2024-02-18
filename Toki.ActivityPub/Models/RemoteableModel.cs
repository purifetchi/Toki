namespace Toki.ActivityPub.Models;

/// <summary>
/// A base model that can be remote.
/// </summary>
public abstract class RemoteableModel : AbstractModel
{
    /// <summary>
    /// When was this object created?
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// The remote ID of this object.
    /// </summary>
    public string? RemoteId { get; init; }
}