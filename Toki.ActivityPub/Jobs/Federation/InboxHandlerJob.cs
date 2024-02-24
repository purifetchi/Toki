using System.Text.Json;
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
    /// <param name="objectJson">The activity json.</param>
    public async Task HandleActivity(string objectJson)
    {
        var activity = JsonSerializer.Deserialize<ASObject>(objectJson) as ASActivity;
        Console.WriteLine($"Handling activity {activity!.Type}");

        // Resolve the actor that's doing this.
        var actor = await resolver.Fetch<ASActor>(activity!.Actor);
        if (actor is null)
            return;

        Console.WriteLine($"Actor {actor.Name}");

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