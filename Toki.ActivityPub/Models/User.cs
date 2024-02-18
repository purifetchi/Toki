using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Models;

/// <summary>
/// A user.
/// </summary>
public class User : RemoteableModel
{
    /// <summary>
    /// The display name of the user.
    /// </summary>
    public required string DisplayName { get; set; }
    
    /// <summary>
    /// The handle of this user. (For remote users it's of the form @someone@something.tld)
    /// </summary>
    public required string Handle { get; set; }
    
    /// <summary>
    /// The bio of this user.
    /// </summary>
    public string? Bio { get; set; }
    
    /// <summary>
    /// Is this user remote?
    /// </summary>
    public bool IsRemote { get; set; }
    
    /// <summary>
    /// The inbox of this user. (Can be null for local users.)
    /// </summary>
    public string? Inbox { get; set; }
    
    /// <summary>
    /// The url of this user's avatar.
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// The url of this user's banner.
    /// </summary>
    public string? BannerUrl { get; set; }
    
    /// <summary>
    /// The keypair of the user.
    /// </summary>
    public required Keypair Keypair { get; init; }

    /// <summary>
    /// Returns this user as an <see cref="ASActor"/>
    /// </summary>
    /// <returns>The resulting actor.</returns>
    public ASActor ToASActor()
    {
        return new ASActor
        {
            // TODO: This is not the proper id of the actor.
            Id = RemoteId ?? $"{Id}",
            
            Name = DisplayName,
            Bio = Bio,
            
            Inbox = Inbox
        };
    }
}