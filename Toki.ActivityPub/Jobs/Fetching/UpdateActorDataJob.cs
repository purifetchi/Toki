using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Jobs.Fetching;

/// <summary>
/// A job that takes care of updating an actor's data in the background.
/// </summary>
public class UpdateActorDataJob(
    ActivityPubResolver resolver,
    UserRepository userRepository)
{
    /// <summary>
    /// Updates an actor.
    /// </summary>
    /// <param name="remoteId">The id of the actor.</param>
    public async Task UpdateActor(string remoteId)
    {
        var user = await userRepository.FindByRemoteId(remoteId);
        if (user is null)
            return;
        
        var asActor = await resolver.Fetch<ASActor>(
            ASObject.Link(remoteId));

        if (asActor is null)
            return;

        await userRepository.UpdateFromActivityStreams(user, asActor);
    }
}