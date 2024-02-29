using System.Text.Json;
using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityPub.Users;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Jobs.Federation;

/// <summary>
/// An inbox handler job.
/// </summary>
public class InboxHandlerJob(
    ActivityPubResolver resolver,
    UserRelationService userRelationService,
    UserRepository repo,
    PostRepository postRepo,
    ILogger<InboxHandlerJob> logger)
{
    /// <summary>
    /// Handles an activity.
    /// </summary>
    /// <param name="objectJson">The activity json.</param>
    public async Task HandleActivity(string objectJson)
    {
        var activity = JsonSerializer.Deserialize<ASObject>(objectJson) as ASActivity;
        logger.LogInformation($"Received activity of type {activity?.Type} from {activity?.Actor.Id}");

        // Ensure we have the actual actor that is performing this task.
        if (await repo.FindByRemoteId(activity!.Actor.Id) is null)
        {
            var actor = await resolver.Fetch<ASActor>(activity!.Actor);
            if (actor is null)
                return;

            await repo.ImportFromActivityStreams(actor);
        }
        
        // TODO: Handle every activity.
        await (activity switch
        {
            ASAccept accept => HandleAccept(accept),
            ASCreate create => HandleCreate(create),
            ASFollow follow => userRelationService.HandleFromActivityStreams(follow),
            _ => Task.Run(() =>
            {
                logger.LogWarning($"Dropped {activity.Id}, due to no handler present for {activity.Type}");
            })
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
        
    }
    
    /// <summary>
    /// Handles the Accept activity.
    /// </summary>
    private async Task HandleAccept(ASAccept accept)
    {
        if (accept.Object is null)
            return;

        if (await userRelationService.TryHandleRemoteFollowAccept(accept))
            return;
        
        logger.LogWarning($"Accept for unknown object {accept.Object.Id}.");
    }
}