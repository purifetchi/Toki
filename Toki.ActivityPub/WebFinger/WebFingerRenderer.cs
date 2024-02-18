using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Persistence.Repositories;

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
        
        var uri = new Uri(queryPath);
        if (uri.Scheme is not acctScheme)
            return null;

        var handle = uri.LocalPath;
        if (!handle.EndsWith(opts.Value.Domain))
            return null;

        handle = handle.Replace($"@{opts.Value.Domain}", "");
        var user = await repo.FindByHandle(handle);

        if (user is null)
            return null;
        
        return new WebFingerResponse
        {
            Subject = uri.LocalPath,
            
            Links = [
                // TODO
            ]
        };
    }
}