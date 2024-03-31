namespace Toki.ActivityPub.Models.Users;

/// <summary>
/// A user profile field.
/// </summary>
public class UserProfileField
{
    /// <summary>
    /// The name of the field.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// The value of the field.
    /// </summary>
    public string? Value { get; set; }
    
    /// <summary>
    /// When was this field verified.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; set; }
}