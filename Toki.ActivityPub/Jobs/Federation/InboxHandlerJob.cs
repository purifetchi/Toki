using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Jobs.Federation;

/// <summary>
/// An inbox handler job.
/// </summary>
public class InboxHandlerJob(
    ActivityPubResolver resolver,
    UserRepository repo)
{
    /// <summary>
    /// Handles an activity.
    /// </summary>
    /// <param name="asObject">The ASObject.</param>
    public async Task HandleActivity(ASObject asObject)
    {
        if (asObject is not ASActivity activity)
            return;

        // Resolve the actor that's doing this.
        var actor = await resolver.Fetch<ASActor>(activity.Actor);
        if (actor is null)
            return;

        // Ensure we have the actual actor that is performing this task.
        if (await repo.FindByRemoteId(actor.Id) is null)
            await repo.ImportFromActivityStreams(actor);
        
        // TODO: Handle every activity.
        await (activity switch
        {
            ASCreate create => HandleCreate(create),
            _ => Task.CompletedTask
        });
    }

    /// <summary>
    /// Handles the Create activity.
    /// </summary>
    private async Task HandleCreate(ASCreate create)
    {
        if (create.Object is null)
            return;

        var obj = await resolver.Fetch<ASObject>(create.Object);
        
        // TODO: Handle the create activity.
    }
}