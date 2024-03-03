using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Models;
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

    /// <summary>
    /// Creates a new inactive token for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="app">The app.</param>
    /// <param name="scopes">The scopes of the token.</param>
    /// <returns>The token, if it was successfully created.</returns>
    public async Task<OAuthToken?> CreateInactiveToken(
        User user,
        OAuthApp app,
        List<string> scopes)
    {
        if (!app.ValidateScopes(scopes) || user.IsRemote)
            return null;
        
        var token = new OAuthToken()
        {
            Id = Guid.NewGuid(),

            ParentApp = app,
            ParentAppId = app.Id,

            User = user,
            UserId = user.Id,

            AuthorizationCode = SecureRandomStringGenerator.Generate(),
            Token = SecureRandomStringGenerator.Generate(),
            
            Scopes = scopes
        };
        
        logger.LogInformation($"Creating an OAuth2 token for user {user.Handle}, with the app {app.ClientName}.");

        await repo.AddToken(token);
        return token;
    }

    /// <summary>
    /// Activates a token.
    /// </summary>
    /// <param name="token">The token.</param>
    public async Task ActivateToken(OAuthToken token)
    {
        token.Active = true;
        await repo.UpdateToken(token);
    }
}