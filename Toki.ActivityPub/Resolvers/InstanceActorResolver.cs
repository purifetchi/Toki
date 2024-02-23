using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Resolvers;

/// <summary>
/// The resolver for the instance actor.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="repo">The user repository.</param>
/// <param name="renderer">The user renderer.</param>
public class InstanceActorResolver(
    TokiDatabaseContext db,
    UserRepository repo,
    UserRenderer renderer)
{
    /// <summary>
    /// The name of the instance actor.
    /// </summary>
    private const string INSTANCE_ACTOR_NAME = "toki.instance.actor";
    
    /// <summary>
    /// The keypair of the instance actor.
    /// </summary>
    private Keypair? _keypair;

    /// <summary>
    /// Fetches the instance actor keypair for this instance.
    /// </summary>
    /// <returns>The keypair.</returns>
    public async Task<Keypair> GetInstanceActorKeypair()
    {
        if (_keypair is not null)
            return _keypair;

        var actor = await GetInstanceActor();
        _keypair = actor.Keypair!;

        return _keypair;
    }

    /// <summary>
    /// Renders the instance actor as an ASActor.
    /// </summary>
    /// <returns>The resulting ASActor.</returns>
    public async Task<ASActor> RenderInstanceActor()
    {
        var actor = await GetInstanceActor();
        var rendered = await renderer.RenderFullActorFrom(
            actor,
            "Application");

        rendered.Invisible = true;
        rendered.Bio = "This is the instance actor used for signed fetches for this Toki instance. Beep boop.";
        return rendered;
    }

    /// <summary>
    /// Gets the instance actor.
    /// </summary>
    /// <returns>The instance actor.</returns>
    private async Task<User> GetInstanceActor()
    {
        var user = await repo.FindByHandle(INSTANCE_ACTOR_NAME);
        if (user is not null)
            return user;

        user = await repo.CreateNewUser(INSTANCE_ACTOR_NAME);
        return user!;
    }
}