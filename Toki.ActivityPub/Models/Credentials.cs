namespace Toki.ActivityPub.Models;

/// <summary>
/// The credentials for a local user.
/// </summary>
public class Credentials : AbstractModel
{
    /// <summary>
    /// The user these credentials are for.
    /// </summary>
    public required User User { get; init; }
    
    /// <summary>
    /// The id of said user.
    /// </summary>
    public required Ulid UserId { get; init; }
    
    /// <summary>
    /// The hash of the password.
    /// </summary>
    public required string PasswordHash { get; init; }
    
    /// <summary>
    /// The salt of the password.
    /// </summary>
    public required string Salt { get; init; }
}