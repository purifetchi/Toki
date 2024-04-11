using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Resolvers;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Objects;
using Toki.HTTPSignatures;
using Toki.HTTPSignatures.Models;

namespace Toki.ActivityPub.Cryptography;

/// <summary>
/// The message validation service for ActivityPub messages.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="resolver">The AP resolver.</param>
/// <param name="validator">The HTTP signature validator.</param>
/// <param name="repo">The user repository.</param>
public class ActivityPubMessageValidationService(
    TokiDatabaseContext db,
    ActivityPubResolver resolver,
    HttpSignatureValidator validator,
    UserRepository repo,
    ILogger<ActivityPubMessageValidationService> logger)
{
    /// <summary>
    /// Validates a message.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="asObject">The object to validate.</param>
    /// <returns>Whether it's valid or not.</returns>
    public async ValueTask<MessageValidationResponse> Validate(
        HttpRequest request,
        ASObject? asObject)
    {
        if (asObject is not ASActivity activity)
        {
            logger.LogWarning(
                $"Tried to validate an ASObject, but it's not an activity! Is it some type we don't support yet? {asObject?.Type}");
            return MessageValidationResponse.NotActivity;
        }

        var signature = Signature.FromHttpRequest(request);

        if (signature is null)
        {
            logger.LogWarning("Couldn't parse signature from HTTP headers!");
            return MessageValidationResponse.CannotParseSignature;
        }
        
        var (response, keypair) = await FetchKeypair(
            signature,
            activity);

        if (keypair is null)
        {
            logger.LogWarning("Couldn't fetch the keypair!");
            return response ?? MessageValidationResponse.GenericError;
        }
        
        return validator.Validate(
            signature,
            keypair!.PublicKey) ? 
            MessageValidationResponse.Ok :
            MessageValidationResponse.ValidationFailed;
    }

    /// <summary>
    /// Fetches a keypair for the given actor.
    /// </summary>
    /// <param name="sig"></param>
    /// <param name="activity"></param>
    /// <returns></returns>
    private async Task<(MessageValidationResponse?, Keypair?)> FetchKeypair(
        Signature sig,
        ASActivity activity)
    {
        var keypair = await db.Keypairs.Where(k => k.RemoteId == sig.KeyId)
            .Include(keypair => keypair.Owner)
            .FirstOrDefaultAsync();

        if (keypair is not null)
        {
            // Verify that the owner is actually the owner we want.
            if (keypair.Owner?.RemoteId != activity.Actor.Id)
                logger.LogWarning($"Actor {activity.Actor.Id} wanted to sign their request with a key for {keypair.Owner?.RemoteId}.");
            return keypair.Owner?.RemoteId != activity.Actor.Id ? 
                (MessageValidationResponse.KeyIdMismatch, null) : 
                (MessageValidationResponse.Ok, keypair);
        }

        var resolvedActor = await resolver.Fetch<ASActor>(activity.Actor);
        
        // If the actor for this is null and we've gotten a 'Delete', that means we've probably
        // just gotten a bogus Mastodon mass-mailed response.
        if (activity is ASDelete)
        {
            logger.LogWarning($"Actor {activity.Actor.Id} was probably removed by a Mastodon instance.");
            return (MessageValidationResponse.MastodonDeleteForUnknownUser, null);
        }
        
        // Fetch the actor, if we don't have them.
        if (resolvedActor?.PublicKey is null)
        {
            logger.LogWarning($"Actor {activity.Actor.Id} doesn't have a public key.");
            return (MessageValidationResponse.InvalidActor, null);
        }
        
        // Ensure the key id isn't fake.
        if (resolvedActor.PublicKey?.Id != sig.KeyId)
        {
            logger.LogWarning($"Key {resolvedActor.PublicKey?.Id} doesn't match the key from the signature! {sig.KeyId}.");
            return (MessageValidationResponse.KeyIdMismatch, null);
        }
        
        var user = await repo.ImportFromActivityStreams(resolvedActor);
        return (MessageValidationResponse.Ok, user?.Keypair);
    }
}