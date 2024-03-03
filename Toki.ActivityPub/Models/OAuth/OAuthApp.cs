namespace Toki.ActivityPub.Models.OAuth;

/// <summary>
/// An OAuth2 app.
/// </summary>
public class OAuthApp : AbstractModel
{
    /// <summary>
    /// The client name.
    /// </summary>
    public required string ClientName { get; set; }
    
    /// <summary>
    /// The client id.
    /// </summary>
    public required string ClientId { get; set; }
    
    /// <summary>
    /// The client secret.
    /// </summary>
    public required string ClientSecret { get; set; }
    
    /// <summary>
    /// The scopes of the app.
    /// </summary>
    public required List<string> Scopes { get; set; }
    
    /// <summary>
    /// The website of this app.
    /// </summary>
    public string? Website { get; set; }
    
    /// <summary>
    /// The redirect URIs.
    /// </summary>
    public List<string>? RedirectUris { get; set; }

    /// <summary>
    /// Validates whether the parameter is a subset of <see cref="Scopes"/>.
    /// </summary>
    /// <param name="scopes">The scopes we want to check.</param>
    /// <returns>True if the scopes fit as a subset.</returns>
    public bool ValidateScopes(IEnumerable<string> scopes) =>
        !scopes.Except(Scopes).Any();
}