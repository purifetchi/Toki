using System.Net;
using System.Net.Sockets;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;
using Toki.HTTPSignatures;

namespace Toki.ActivityPub.Jobs.Federation;

/// <summary>
/// The message federation job.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class MessageFederationJob(
    SignedHttpClient httpClient,
    InstancePathRenderer pathRenderer,
    IOptions<InstanceConfiguration> opts,
    UserRepository userRepo,
    ILogger<MessageFederationJob> logger)
{
    /// <summary>
    /// The maximum amount of retries.
    /// </summary>
    private const int MAX_RETRIES_COUNT = 10;

    /// <summary>
    /// Calculates the delay for the next retry based on exponential falloff.
    /// </summary>
    /// <param name="retry">The retry number.</param>
    /// <returns>The delay for the next call.</returns>
    private TimeSpan CalculateExponentialBackoff(int retry)
    {
        const int interval = 5;
        const float exponent = 2.5f;

        var accum = 0f;
        for (var i = 0; i < retry; i++)
            accum += interval * MathF.Pow(exponent, i);
        
        return TimeSpan.FromSeconds(accum);
    }
    
    /// <summary>
    /// Federates a message to the selected targets.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="targets">The targets to federate to.</param>
    /// <param name="retries">The amount of retries.</param>
    /// <param name="actorId">The actor id.</param>
    public async Task FederateMessage(
        string message,
        IEnumerable<string> targets,
        Ulid actorId,
        int retries = 0)
    {
        var actor = await userRepo.FindById(actorId);
        var keypair = actor!.Keypair!;
        
        const int secondsPerRetry = 5;
        
        var failed = new List<string>();
        logger.LogInformation($"httpClient: {httpClient}, keypair: {keypair?.Id}, publickey: {keypair?.PublicKey}, message: {message}");
        
        foreach (var target in targets)
        {
            logger.LogInformation($"Delivering message to {target}");
            
            // TODO: We should probably precalculate the digest, so we don't have to calculate it
            //       every single time we send to a different endpoint.
            HttpResponseMessage result;
            try
            {
                result = await httpClient
                    .NewRequest()
                    .WithKey(
                        keypair!.RemoteId ?? $"{pathRenderer.GetPathToActor(actor!)}#key",
                        keypair.PrivateKey!)
                    .WithBody(message)
                    .WithHeader("User-Agent", opts.Value.UserAgent)
                    .AddHeaderToSign("Host")
                    .AddHeaderToSign("Digest")
                    .SetDate(DateTimeOffset.UtcNow.AddSeconds(5))
                    .Post(target);
            }
            catch (HttpRequestException e)
            {
                logger.LogWarning($"Delivering to {target} failed! Request exception: {e}.");
                failed.Add(target);

                continue;
            }
            catch (SocketException e)
            {
                logger.LogWarning($"Delivering to {target} failed! Socket exception: {e}.");
                failed.Add(target);

                continue;
            }

            if (result.IsSuccessStatusCode)
                continue;

            // If the server returns a 410 Gone, it means that the instance behind it is shut down.
            // Do not attempt to retransmit the message.
            // TODO: Should 404 cause the same behavior?
            if (result.StatusCode == HttpStatusCode.Gone)
            {
                logger.LogWarning($"{target} reported itself as being shut down.");
                continue;
            }

            logger.LogWarning($"Delivering to {target} failed! Status code: {result.StatusCode}, response: {await result.Content.ReadAsStringAsync()}");
            failed.Add(target);
        }
        
        retries++;

        if (retries > MAX_RETRIES_COUNT || failed.Count < 1)
            return;
        
        BackgroundJob.Schedule<MessageFederationJob>(job =>
            job.FederateMessage(message, failed, actorId, retries), 
            CalculateExponentialBackoff(retries));
    }
}