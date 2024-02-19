using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Renderers;

/// <summary>
/// The user renderer, responsible for rendering from our models to ActivityStreams.
/// </summary>
/// <param name="db">The database.</param>
/// <param name="opts">The options.</param>
public class UserRenderer(
    TokiDatabaseContext db,
    IOptions<InstanceConfiguration> opts)
{
    /// <summary>
    /// Renders a full <see cref="ASActor"/> from a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The actor.</returns>
    public async Task<ASActor> RenderFullActorFrom(User user)
    {
        var domain = opts.Value.Domain;
        var key = await db.Keypairs
            .FirstOrDefaultAsync(kp => kp.Owner == user)!;

        var uri = GetPublicUri(user);
        
        return new ASActor
        {
            Id = user.RemoteId ?? uri,
            
            Name = user.DisplayName,
            Bio = user.Bio,
            
            Inbox = user.Inbox ?? $"{uri}/inbox",
            
            PublicKey = new ASPublicKey
            {
                Id = key!.RemoteId ?? $"{uri}/key",
                PublicKeyPem = key!.PublicKey
            },
            
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
        return ASObject.Link(user.RemoteId ?? GetPublicUri(user));
    }
    
    /// <summary>
    /// Gets the public URI for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The URI.</returns>
    private string GetPublicUri(User user)
    {
        return $"https://{opts.Value.Domain}/users/{user.Handle}";
    }
}