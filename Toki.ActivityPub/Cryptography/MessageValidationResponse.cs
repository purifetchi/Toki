namespace Toki.ActivityPub.Cryptography;

public enum MessageValidationResponse
{
    /// <summary>
    /// Validation has passed.
    /// </summary>
    Ok,
    
    /// <summary>
    /// This message wasn't an activity.
    /// </summary>
    NotActivity,
    
    /// <summary>
    /// We couldn't parse the signature.
    /// </summary>
    CannotParseSignature,

    /// <summary>
    /// The key id was mismatched.
    /// </summary>
    KeyIdMismatch,
    
    /// <summary>
    /// The message validation failed
    /// </summary>
    ValidationFailed,
    
    /// <summary>
    /// A generic error.
    /// </summary>
    GenericError,
    
    /// <summary>
    /// We've gotten an invalid actor.
    /// </summary>
    InvalidActor,
    
    /// <summary>
    /// This is a Mastodon mass-mailed delete for a user we don't care about.
    /// </summary>
    MastodonDeleteForUnknownUser
}