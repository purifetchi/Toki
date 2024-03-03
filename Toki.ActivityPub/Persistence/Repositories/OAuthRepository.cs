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
}