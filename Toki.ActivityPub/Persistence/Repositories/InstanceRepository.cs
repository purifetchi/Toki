using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.NodeInfo;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Persistence.Repositories;

/// <summary>
/// The repository for instances.
/// </summary>
/// <param name="db">The database.</param>
public class InstanceRepository(
    TokiDatabaseContext db,
    NodeInfoResolver resolver,
    ILogger<InstanceRepository> logger)
{
    /// <summary>
    /// Adds an instance to the repo.
    /// </summary>
    /// <param name="instance">The instance.</param>
    public async Task Add(RemoteInstance instance)
    {
        db.Instances.Add(instance);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Gets an instance by its domain.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns>The instance, if it exists.</returns>
    public async Task<RemoteInstance?> GetByDomain(string domain)
    {
        return await db.Instances
            .Where(i => i.Domain == domain)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets the instance for an ASActor.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <returns>The instance, if it exists.</returns>
    public async Task<RemoteInstance?> GetForActor(ASActor actor)
    {
        return await GetByDomain(
            new Uri(actor.Id).Host);
    }

    /// <summary>
    /// Gets all of the connected instances.
    /// </summary>
    /// <returns>The list of connected instances.</returns>
    public async Task<IEnumerable<RemoteInstance>> GetAllConnectedInstances()
    {
        return await db.Instances
            .ToListAsync();
    }
    
    /// <summary>
    /// Fetches an instance for a given actor.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <returns>The created instance.</returns>
    public async Task<RemoteInstance> FetchInstanceForActor(ASActor actor)
    {
        var domain = new Uri(actor.Id)
            .Host;

        var nodeInfo = await resolver.Get(domain);
        var instance = new RemoteInstance()
        {
            Id = Ulid.NewUlid(),
            
            Domain = domain,
            SharedInbox = actor.Endpoints?.SharedInbox?.Id,
            
            Name = nodeInfo?.Metadata?.Name,
            Description = nodeInfo?.Metadata?.Description,
            
            Software = $"{nodeInfo?.Software?.Name} {nodeInfo?.Software?.Version}"
        };
        
        logger.LogInformation($"Met new instance! {instance.Domain} [{instance.Software}]");

        await Add(instance);

        return instance;
    }
}