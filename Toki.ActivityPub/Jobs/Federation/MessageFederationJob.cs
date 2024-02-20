using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Models;
using Toki.HTTPSignatures;

namespace Toki.ActivityPub.Jobs.Federation;

/// <summary>
/// The message federation job.
/// </summary>
public class MessageFederationJob(
    SignedHttpClient httpClient,
    IOptions<InstanceConfiguration> opts)
{
    /// <summary>
    /// The maximum amount of retries.
    /// </summary>
    private const int MAX_RETRIES_COUNT = 10;
    
    /// <summary>
    /// Federates a message to the selected targets.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="targets">The targets to federate to.</param>
    /// <param name="retries">The amount of retries.</param>
    /// <param name="keypair">The keypair.</param>
    public async Task FederateMessage(
        string message,
        List<string> targets,
        int retries,
        Keypair keypair)
    {
        var failed = new List<string>();
        httpClient.WithKey(
                keypair.RemoteId ?? $"https://{opts.Value.Domain}/users/{keypair.Owner!.Id}/key", 
                keypair.PublicKey)
            .WithBody(message)
            .WithHeader("User-Agent", $"Toki ({opts.Value.Domain}; <{opts.Value.ContactEmail}>)")
            .AddHeaderToSign("Host")
            .AddHeaderToSign("Digest")
            .AddHeaderToSign("Date", 
                DateTimeOffset.UtcNow.AddSeconds(5).ToString("D, d M Y H:i:s T"));

        foreach (var target in targets)
        {
            var result = await httpClient.Post(target);
            if (!result.IsSuccessStatusCode)
                failed.Add(target);
        }
        
        retries++;

        if (retries > MAX_RETRIES_COUNT)
            return;
        
        // TODO: Reschedule.
    }
}