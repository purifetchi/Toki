using Toki.ActivityPub.Models;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Emojis;

/// <summary>
/// The service responsible for managing emojis.
/// </summary>
/// <param name="repo">The emoji repository.</param>
public class EmojiService(
    EmojiRepository repo)
{
    /// <summary>
    /// Creates an emoji from an <see cref="ASEmoji"/>
    /// </summary>
    /// <param name="emoji">The emoji.</param>
    /// <param name="parentInstance">The parent instance.</param>
    /// <returns>The emoji.</returns>
    private Emoji? EmojiFromActivityStreams(
        ASEmoji emoji,
        RemoteInstance? parentInstance)
    {
        if (emoji.Name is null)
            return null;

        if (emoji.Icon?.Url is null)
            return null;

        return new Emoji
        {
            Id = Ulid.NewUlid(),

            Shortcode = emoji.Name,
            RemoteUrl = emoji.Icon!.Url!,

            ParentInstance = parentInstance,
            ParentInstanceId = parentInstance?.Id
        };
    }
    
    /// <summary>
    /// Fetches Toki emojis from the ActivityStreams emoji list.
    /// </summary>
    /// <param name="list">The list of <see cref="ASEmoji"/></param>
    /// <param name="instance">The instance these emojis belong to.</param>
    /// <returns>The list of <see cref="Emoji"/></returns>
    public async Task<IReadOnlyList<Emoji>> FetchFromActivityStreams(
        IReadOnlyList<ASEmoji> list,
        RemoteInstance? instance = null)
    {
        var names = list
            .Where(e => e.Name is not null)
            .Select(e => e.Name!)
            .ToList();

        var all = new List<Emoji>();
        
        all.AddRange(
            await repo.FindManyByNameAndInstance(
                names,
                instance)
            );
        
        var missing = new List<Emoji>();
        foreach (var missingEmoji in list)
        {
            if (all.Any(e => e.Shortcode == missingEmoji.Name))
                continue;
            
            var emoji = EmojiFromActivityStreams(
                missingEmoji,
                instance);

            if (emoji is null)
                continue;
            
            missing.Add(emoji);
        }

        await repo.AddMany(
            missing);

        all.AddRange(missing);
        
        return all;
    }

    /// <summary>
    /// Creates a local emoji.
    /// </summary>
    /// <param name="shortcode">The shortcode.</param>
    /// <param name="url">The url.</param>
    /// <returns>The created emoji.</returns>
    public async Task<Emoji?> CreateLocalEmoji(
        string shortcode,
        string url)
    {
        var emoji = new Emoji
        {
            Id = Ulid.NewUlid(),

            Shortcode = $":{shortcode}:",
            RemoteUrl = url
        };

        await repo.Add(emoji);
        return emoji;
    }

    /// <summary>
    /// Gets all of the local emoji.
    /// </summary>
    /// <returns>The emoji.</returns>
    public async Task<IEnumerable<Emoji>> GetAllLocalEmoji()
    {
        return await repo.FindManyByInstance(
            null);
    }
}