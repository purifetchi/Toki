using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Renderers;
using Toki.HTTPSignatures;

namespace Toki.ActivityPub.Jobs.Federation;

/// <summary>
/// The message federation job.
/// </summary>
public class MessageFederationJob(
    SignedHttpClient httpClient,
    InstancePathRenderer pathRenderer,
    IOptions<InstanceConfiguration> opts,
    ILogger<MessageFederationJob> logger)
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
        IEnumerable<string> targets,
        Keypair keypair,
        int retries = 0)
    {
        const int secondsPerRetry = 5;
        
        var failed = new List<string>();
        httpClient.WithKey(
                keypair.RemoteId ?? $"{pathRenderer.GetPathToActor(keypair.Owner!)}#key", 
                keypair.PublicKey)
            .WithBody(message)
            .WithHeader("User-Agent", $"Toki ({opts.Value.Domain}; <{opts.Value.ContactEmail}>)")
            .AddHeaderToSign("Host")
            .AddHeaderToSign("Digest")
            .AddHeaderToSign("Date", 
                DateTimeOffset.UtcNow.AddSeconds(5).ToString("D, d M Y H:i:s T"));

        foreach (var target in targets)
        {
            logger.LogInformation($"Delivering message to {target}");
            var result = await httpClient.Post(target);

            if (result.IsSuccessStatusCode)
                continue;

            logger.LogWarning($"Delivering to {target} failed!");
            failed.Add(target);
        }
        
        retries++;

        if (retries > MAX_RETRIES_COUNT)
            return;
        
        BackgroundJob.Schedule<MessageFederationJob>(job =>
            job.FederateMessage(message, failed, keypair, retries), 
            TimeSpan.FromSeconds(retries * secondsPerRetry));
    }
}