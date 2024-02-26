using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Cryptography;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityPub.Renderers;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// The user repository.
/// </summary>
public class UserRepository(
    TokiDatabaseContext db,
    InstanceRepository instanceRepo,
    InstancePathRenderer pathRenderer,
    ILogger<UserRepository> logger)
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
    public async Task<bool> Add(User user)
    {
        db.Users.Add(user);
        var changes = await db.SaveChangesAsync();
        
        return changes > 0;
    }
    
    /// <summary>
    /// Imports a user from the ActivityStreams actor definition.
    /// </summary>
    /// <param name="actor">The actor.</param>
    public async Task<User?> ImportFromActivityStreams(ASActor actor)
    {
        if (actor.PublicKey is null || !actor.PublicKey.IsResolved)
            return null;
        
        logger.LogInformation($"Creating remote user {actor.Name} ({actor.Id})");

        var instance = await instanceRepo.GetForActor(actor)
                       ?? await instanceRepo.FetchInstanceForActor(actor);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            
            ParentInstance = instance,
            ParentInstanceId = instance.Id,
            
            DisplayName = actor.Name!,
            RemoteId = actor.Id!,
            
            IsRemote = true,
            
            Inbox = actor.Inbox?.Id,
            Bio = actor.Bio,
            
            AvatarUrl = actor.Icon?.Url,
            BannerUrl = actor.Banner?.Url,
            
            RequiresFollowApproval = actor.ManuallyApprovesFollowers,
            
            Keypair = new Keypair
            {
                Id = Guid.NewGuid(),
            
                RemoteId = actor.PublicKey!.Id,
                PublicKey = actor.PublicKey!.PublicKeyPem!
            },
            
            Handle = $"{actor.PreferredUsername!}@{instance.Domain}"
        };

        user.Keypair.Owner = user;

        await Add(user);

        return user;
    }

    /// <summary>
    /// Creates a new user with a given handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <param name="password">The password of the new user.</param>
    /// <returns>The created user.</returns>
    public async Task<User?> CreateNewUser(
        string handle,
        string? password = null)
    {
        if (await FindByHandle(handle) != null)
            return null;
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            
            DisplayName = handle,
            IsRemote = false,
            
            RemoteId = pathRenderer.GetPathToActor(handle),
            
            Keypair = KeypairGenerationHelper.GenerateKeypair(),
            Handle = handle
        };

        user.Keypair.Owner = user;
        db.Users.Add(user);

        if (password is not null)
        {
            const int workFactor = 8;
            var salt = BCrypt.Net.BCrypt.GenerateSalt(workFactor);
            var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);
        
            var credentials = new Credentials()
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = user.Id,

                PasswordHash = hash,
                Salt = salt
            };
        
            db.Credentials.Add(credentials);
        }
        
        await db.SaveChangesAsync();
        
        return user;
    }
}