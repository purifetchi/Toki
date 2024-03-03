using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;

namespace Toki.ActivityPub.Users;

/// <summary>
/// The service managing user session.
/// </summary>
public class UserSessionService(
    TokiDatabaseContext db)
{
    /// <summary>
    /// Validates the credentials.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>The user, if the credentials match. Null otherwise.</returns>
    public async Task<User?> ValidateCredentials(
        string username,
        string password)
    {
        var credentials = await db.Credentials
            .Include(cred => cred.User)
            .FirstOrDefaultAsync(cred => cred.User.Handle == username);

        if (credentials is null)
            return null;

        var success = BCrypt.Net.BCrypt.Verify(
            password,
            credentials.PasswordHash);

        return success ? credentials.User : null;
    }
}