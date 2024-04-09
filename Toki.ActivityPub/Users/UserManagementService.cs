using Toki.ActivityPub.Federation;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Users;

/// <summary>
/// A service that helps with managing users.
/// </summary>
public class UserManagementService(
    UserRepository repo,
    UserRenderer renderer,
    ActivityPubResolver resolver,
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

    /// <summary>
    /// Fetches a user given their remote id.
    /// </summary>
    /// <param name="remoteId">The remote id of the user.</param>
    /// <returns>The user.</returns>
    public async Task<User?> FetchFromRemoteId(
        string remoteId)
    {
        var maybeUser = await repo.FindByRemoteId(remoteId);
        if (maybeUser is not null)
            return maybeUser;

        var actor = await resolver.Fetch<ASActor>(
            ASObject.Link(remoteId));

        if (actor is null)
            return null;
        
        // TODO: Schedule fetching stuff like the pinned posts here.
        return await repo.ImportFromActivityStreams(
            actor);
    }
}