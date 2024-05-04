using Toki.ActivityPub.Federation;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Notifications;
using Toki.ActivityPub.Renderers;
using Toki.ActivityStreams.Activities.Extensions;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Bites;

/// <summary>
/// Service for dealing with Bite activities.
/// </summary>
public class BiteService(
    UserRenderer userRenderer,
    InstancePathRenderer pathRenderer,
    NotificationService notificationService,
    MessageFederationService federationService)
{
    /// <summary>
    /// Renders a bite for a target object.
    /// </summary>
    /// <param name="actor">The actor biting the target.</param>
    /// <param name="to">The actor who has made the target object.</param>
    /// <param name="target">The target object.</param>
    /// <returns>The ASBite.</returns>
    private ASBite RenderBite(
        User actor,
        User to,
        ASObject target)
    {
        return new ASBite()
        {
            Id = $"{pathRenderer.GetPathToActor(actor)}#bites/{Ulid.NewUlid()}",
            Actor = userRenderer.RenderLinkedActorFrom(actor),
            Target = target,
            To = [to.RemoteId!]
        };
    }
    
    /// <summary>
    /// Bites the target user.
    /// </summary>
    /// <param name="source">The source user biting.</param>
    /// <param name="target">The target user.</param>
    public async Task Bite(
        User source,
        User target)
    {
        if (!target.IsRemote)
        {
            await notificationService.DispatchBite(
                target,
                source);

            return;
        }
        
        if (source.IsRemote)
            return;

        var bite = RenderBite(
            source,
            target,
            userRenderer.RenderLinkedActorFrom(target));

        federationService.SendTo(
            source,
            target,
            bite);
    }
}