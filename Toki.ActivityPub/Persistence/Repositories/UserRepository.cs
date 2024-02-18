using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// The user repository.
/// </summary>
public class UserRepository(TokiDatabaseContext db)
{
    /// <summary>
    /// Finds a user by their id.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns>Said user.</returns>
    public async Task<User?> FindById(Guid id)
    {
        var result = await db.Users.Where(u => u.Id == id)
            .FirstOrDefaultAsync();

        return result;
    }
    
    /// <summary>
    /// Finds a user by their remote id.
    /// </summary>
    /// <param name="id">The remote id.</param>
    /// <returns>The user, if they exist.</returns>
    public async Task<User?> FindByRemoteId(string id)
    {
        var result = await db.Users.Where(u => u.RemoteId == id)
            .FirstOrDefaultAsync();

        return result;
    }

    /// <summary>
    /// Finds a user by their handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>The user, if they exist.</returns>
    public async Task<User?> FindByHandle(string handle)
    {
        var result = await db.Users.Where(u => u.Handle == handle)
            .FirstOrDefaultAsync();

        return result;
    }

    /// <summary>
    /// Adds a new user.
    /// </summary>
    /// <param name="user">Said user.</param>
    public async Task Add(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Imports a user from the ActivityStreams actor definition.
    /// </summary>
    /// <param name="actor">The actor.</param>
    public async Task ImportFromActivityStreams(ASActor actor)
    {
        if (actor.PublicKey is null || !actor.PublicKey.IsResolved)
            return;
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            
            DisplayName = actor.Name!,
            RemoteId = actor.Id!,
            
            Keypair = new Keypair
            {
                Id = Guid.NewGuid(),
            
                RemoteId = actor.PublicKey!.Id,
                PublicKey = actor.PublicKey!.PublicKeyPem!
            },
            
            Handle = actor.Name! // TODO: This should also have the instance baked in.
        };

        user.Keypair.Owner = user;

        await Add(user);
    }
}