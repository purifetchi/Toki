namespace Toki.ActivityPub.Models.OAuth;

/// <summary>
/// An OAuth2 token.
/// </summary>
public class OAuthToken : AbstractModel
{
    /// <summary>
    /// Creates a new OAuth2 token.
    /// </summary>
    public OAuthToken()
    {
        CreatedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// The authorization code.
    /// </summary>
    public required string AuthorizationCode { get; init; }
    
    /// <summary>
    /// The token.
    /// </summary>
    public required string Token { get; init; }
    
    /// <summary>
    /// The parent app of this token.
    /// </summary>
    public required OAuthApp ParentApp { get; init; }
    
    /// <summary>
    /// The id of the parent app.
    /// </summary>
    public required Ulid ParentAppId { get; init; }

    /// <summary>
    /// The user this token is for.
    /// </summary>
    public required User User { get; init; }

    /// <summary>
    /// The id of the user.
    /// </summary>
    public required Ulid UserId { get; init; }
    
    /// <summary>
    /// When was this token created at.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Is this token active?
    /// </summary>
    public bool Active { get; set; }
    
    /// <summary>
    /// The scopes of this token.
    /// </summary>
    public List<string>? Scopes { get; set; }
}