namespace Toki.Middleware.OAuth2;

/// <summary>
/// The attribute decorating any OAuth2 protected resource.
/// </summary>
/// <param name="scope">The scope.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class OAuthAttribute(
    string scope) : Attribute
{
    public string Scope { get; } = scope;
}