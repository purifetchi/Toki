namespace Toki.Middleware.OAuth2;

/// <summary>
/// The attribute decorating any OAuth2 protected resource.
/// </summary>
/// <param name="scope">The scope.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class OAuthAttribute(
    string scope = "",
    bool manualScopeValidation = false) : Attribute
{
    /// <summary>
    /// The scope.
    /// </summary>
    public string Scope { get; } = scope;

    /// <summary>
    /// Should the user manually verify the required scopes?
    /// </summary>
    public bool ManualScopeValidation { get; } = manualScopeValidation;
}