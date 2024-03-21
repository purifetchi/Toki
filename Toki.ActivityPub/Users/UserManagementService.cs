using Toki.ActivityPub.Federation;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;

namespace Toki.ActivityPub.Users;

/// <summary>
/// A service that helps with managing users.
/// </summary>
public class UserManagementService(
    UserRepository repo,
    UserRenderer renderer,
    MessageFederationService federationService)
{
    /// <summary>
    /// Updates a user.
    /// </summary>
    /// <param name="user">The user.</param>
    public async Task Update(User user)
    {
        await repo.Update(user);
        
        // Federate the message.
        var msg = await renderer.RenderUpdateFor(
            user);

        await federationService.SendToFollowers(user, msg);
    }
}