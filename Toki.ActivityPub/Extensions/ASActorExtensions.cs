using Toki.ActivityPub.Models.Users;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Extensions;

/// <summary>
/// Extensions for the <see cref="ASActor"/> class.
/// </summary>
public static class ASActorExtensions
{
    /// <summary>
    /// Gets the <see cref="UserProfileField"/> entities from an actor's attachments.
    /// </summary>
    /// <param name="actor">The ActivityStreams actor.</param>
    /// <returns>The profile fields.</returns>
    public static IList<UserProfileField>? GetUserProfileFields(this ASActor actor) => actor.Attachments?
        .Where(at => at is ASPropertyValue)
        .Cast<ASPropertyValue>()
        .Select(kv => new UserProfileField()
        {
            Name = kv.Name,
            Value = kv.Value
        }).ToList();
}