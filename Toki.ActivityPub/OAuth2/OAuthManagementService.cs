using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Models.OAuth;
using Toki.ActivityPub.Persistence.Repositories;

namespace Toki.ActivityPub.OAuth2;

/// <summary>
/// A management service for OAuth2.
/// </summary>
/// <param name="repo">The oauth repository.</param>
public class OAuthManagementService(
    OAuthRepository repo,
    ILogger<OAuthManagementService> logger)
{
    /// <summary>
    /// Registers an OAuth2 app.
    /// </summary>
    /// <param name="name">The name of the app.</param>
    /// <param name="uris">The redirect uris valid for this app.</param>
    /// <param name="scopes">The scopes the app requested.</param>
    /// <param name="website">The website of the app.</param>
    /// <returns>The app, if it was successfully registered.</returns>
    public async Task<OAuthApp?> RegisterApp(
        string name,
        List<string> uris,
        List<string> scopes,
        string? website)
    {
        var app = new OAuthApp()
        {
            Id = Guid.NewGuid(),

            ClientId = SecureRandomStringGenerator.Generate(),
            ClientSecret = SecureRandomStringGenerator.Generate(),

            ClientName = name,
            RedirectUris = uris,
            Scopes = scopes,
            Website = website
        };
        
        logger.LogInformation($"Registering OAuth2 app {name}.");

        await repo.AddApp(app);
        return app;
    }
}