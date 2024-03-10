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
        ReceivedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// When was this object created?
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// When did we receive this message from the remote server? (identical to <see cref="RemoteableModel.CreatedAt"/> for local posts.)
    /// </summary>
    // This is required, since according to @arnelson@fosstodon.org, mastoapi orders by date received not date of creation.
    public DateTimeOffset ReceivedAt { get; set; }
    
    /// <summary>
    /// The remote ID of this object.
    /// </summary>
    public string? RemoteId { get; set; }
}