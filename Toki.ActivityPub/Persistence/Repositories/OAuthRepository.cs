using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models.OAuth;
using Toki.ActivityPub.Persistence.DatabaseContexts;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// A repository for anything related to OAuth2.
/// </summary>
public class OAuthRepository(
    TokiDatabaseContext db)
{
    /// <summary>
    /// Adds a new OAuth2 app.
    /// </summary>
    /// <param name="app">The app.</param>
    public async Task AddApp(OAuthApp app)
    {
        db.OAuthApps.Add(app);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Adds a new OAuth2 token.
    /// </summary>
    /// <param name="token">The token.</param>
    public async Task AddToken(OAuthToken token)
    {
        db.OAuthTokens.Add(token);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Updates an OAuth2 token.
    /// </summary>
    /// <param name="token">The token.</param>
    public async Task UpdateToken(OAuthToken token)
    {
        db.OAuthTokens.Update(token);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Finds an app by its client id.
    /// </summary>
    /// <param name="clientId">The client id.</param>
    /// <returns>The app.</returns>
    public async Task<OAuthApp?> FindByClientId(string clientId)
    {
        return await db.OAuthApps
            .FirstOrDefaultAsync(app => app.ClientId == clientId);
    }
    
    /// <summary>
    /// Finds a token by its authorization code.
    /// </summary>
    /// <param name="authCode">The auth code.</param>
    /// <returns>The token.</returns>
    public async Task<OAuthToken?> FindTokenByAuthCode(string authCode)
    {
        return await db.OAuthTokens
            .FirstOrDefaultAsync(token => token.AuthorizationCode == authCode);
    }
}