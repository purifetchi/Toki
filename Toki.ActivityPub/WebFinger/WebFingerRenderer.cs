using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Resolvers;

namespace Toki.ActivityPub.WebFinger;

/// <summary>
/// The WebFinger renderer.
/// </summary>
/// <param name="repo">The user repo.</param>
public class WebFingerRenderer(
    UserRepository repo,
    IOptions<InstanceConfiguration> opts)
{
    /// <summary>
    /// Finds a user and renders them into a WebFingerResponse.
    /// </summary>
    /// <returns>The response, if the user was found.</returns>
    public async Task<WebFingerResponse?> FindUser(string queryPath)
    {
        const string acctScheme = "acct";

        var domain = opts.Value.Domain;
        
        var uri = new Uri(queryPath);
        if (uri.Scheme is not acctScheme)
            return null;

        var handle = uri.LocalPath;
        if (!handle.EndsWith(domain))
            return null;

        handle = handle.Replace($"@{domain}", "");
        
        // Check if we're being queried for the instance actor.
        if (handle == domain || 
            handle == InstanceActorResolver.INSTANCE_ACTOR_NAME)
        {
            return new WebFingerResponse
            {
                Subject = $"acct:{InstanceActorResolver.INSTANCE_ACTOR_NAME}@{domain}",
                Aliases = [$"https://{domain}/actor"],
                Links = [
                    new WebFingerLink
                    {
                        Hyperlink = $"https://{domain}/actor",
                        Type = "application/activity+json",
                        Relative = "self"
                    },
                
                    new WebFingerLink
                    {
                        Hyperlink = $"https://{domain}/actor",
                        Type = "application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"",
                        Relative = "self"
                    }
                ]
            };
        }
        
        var user = await repo.FindByHandle(handle);

        if (user is null)
            return null;
        
        return new WebFingerResponse
        {
            Subject = queryPath,
            
            Links = [
                new WebFingerLink
                {
                    Hyperlink = $"https://{domain}/users/{user.Handle}",
                    Type = "application/activity+json",
                    Relative = "self"
                },
                
                new WebFingerLink
                {
                    Hyperlink = $"https://{domain}/users/{user.Handle}",
                    Type = "application/ld+json; profile=\"https://www.w3.org/ns/activitystreams\"",
                    Relative = "self"
                }
            ]
        };
    }
}