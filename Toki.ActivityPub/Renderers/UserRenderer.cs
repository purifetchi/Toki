using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Renderers;

/// <summary>
/// The user renderer, responsible for rendering from our models to ActivityStreams.
/// </summary>
/// <param name="db">The database.</param>
/// <param name="opts">The options.</param>
public class UserRenderer(
    TokiDatabaseContext db,
    InstancePathRenderer pathRenderer,
    IOptions<InstanceConfiguration> opts)
{
    /// <summary>
    /// Renders a full <see cref="ASActor"/> from a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="type">The type of the actor.</param>
    /// <returns>The actor.</returns>
    public async Task<ASActor> RenderFullActorFrom(
        User user,
        string type = "Person") // TODO: The type shouldn't be passed in as a string, probably.
    {
        var domain = opts.Value.Domain;
        var key = await db.Keypairs
            .FirstOrDefaultAsync(kp => kp.Owner == user)!;

        var uri = pathRenderer.GetPathToActor(user);
        
        return new ASActor(type)
        {
            Id = user.RemoteId ?? uri,
            
            Name = user.DisplayName,
            Bio = user.Bio,
            
            PreferredUsername = user.Handle,
            
            Inbox = ASObject.Link(user.Inbox ?? $"{user.RemoteId ?? uri}/inbox"),
            Outbox = ASObject.Link($"{user.RemoteId ?? uri}/outbox"),
            
            Followers = ASObject.Link($"{user.RemoteId ?? uri}/followers"),
            Following = ASObject.Link($"{user.RemoteId ?? uri}/following"),
            
            PublicKey = new ASPublicKey
            {
                Id = key!.RemoteId ?? $"{user.RemoteId ?? uri}#key",
                Owner = ASObject.Link(user.RemoteId ?? uri),
                PublicKeyPem = key!.PublicKey
            },
            
            Icon = user.AvatarUrl is not null ? new ASImage()
            {
                Url = user.AvatarUrl!
            } : null,
            
            Banner = user.BannerUrl is not null ? new ASImage()
            {
                Url = user.BannerUrl!
            } : null,
            
            Endpoints = new ASEndpoints
            {
                SharedInbox = ASObject.Link($"https://{domain}/inbox")
            }
        };
    }

    /// <summary>
    /// Renders a link to an actor from a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The actor.</returns>
    public ASObject RenderLinkedActorFrom(User user)
    {
        return ASObject.Link(
            user.RemoteId ?? 
            pathRenderer.GetPathToActor(user));
    }

    /// <summary>
    /// Renders an <see cref="ASUpdate"/> for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The update.</returns>
    public async Task<ASUpdate> RenderUpdateFor(User user)
    {
        var link = RenderLinkedActorFrom(user);
        return new ASUpdate()
        {
            Id = $"{link.Id}#updates/{Ulid.NewUlid()}",

            Actor = link,
            Object = await RenderFullActorFrom(user)
        };
    }
}