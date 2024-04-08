using Toki.MastodonApi.Schemas.Errors;

namespace Toki.MastodonApi.Helpers;

/// <summary>
/// Helper classes for OAuth2 integration.
/// </summary>
public static class MastodonOAuthHelper
{
    /// <summary>
    /// The "read" expanded scopes.
    /// </summary>
    private static IEnumerable<string> ReadScopes { get; } =
    [
        "read:accounts",
        "read:blocks",
        "read:bookmarks",
        "read:favourites",
        "read:filters",
        "read:follows",
        "read:lists",
        "read:mutes",
        "read:notifications",
        "read:search",
        "read:statuses"
    ];
    
    /// <summary>
    /// The "write" expanded scopes.
    /// </summary>
    private static IEnumerable<string> WriteScopes { get; } =
    [
        "write:accounts",
        "write:blocks",
        "write:bookmarks",
        "write:conversations",
        "write:favourites",
        "write:filters",
        "write:follows",
        "write:lists",
        "write:media",
        "write:mutes",
        "write:notifications",
        "write:reports",
        "write:statuses"
    ];
    
    /// <summary>
    /// Validates the request uris.
    /// </summary>
    /// <param name="uris">The uris.</param>
    /// <returns>An error if something went wrong, null otherwise.</returns>
    public static MastodonApiError? ValidateRedirectUris(IEnumerable<string> uris)
    {
        const string noRedirectUri = "urn:ietf:wg:oauth:2.0:oob";
        
        foreach (var uri in uris)
        {
            if (uri == noRedirectUri)
                continue;

            try
            {
                var uriObject = new Uri(uri);
                if (!uriObject.IsAbsoluteUri)
                    return new MastodonApiError("Validation error: URI must be absolute.");

                if (uriObject.Scheme is "javascript" or "file")
                    return new MastodonApiError("Validation error: Invalid scheme for redirect URI.");
            }
            catch (UriFormatException)
            {
                return new MastodonApiError($"Validation error: Improperly formatted URI ({uri}) passed in.");
            }
        }
        
        return null;
    }

    /// <summary>
    /// Expands the scopes.
    /// </summary>
    /// <param name="scopes">The scopes.</param>
    /// <returns>The expanded scopes.</returns>
    public static List<string> ExpandScopes(IEnumerable<string> scopes)
    {
        var returnScopes = new List<string>();
        foreach (var scope in scopes)
        {
            switch (scope)
            {
                case "read":
                    returnScopes.AddRange(ReadScopes);
                    break;
                
                case "write":
                    returnScopes.AddRange(WriteScopes);
                    break;
                
                default:
                    returnScopes.Add(scope);
                    break;
            }
        }

        return returnScopes
            .Distinct()
            .ToList();
    }
}