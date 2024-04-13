using System.Text.RegularExpressions;
using System.Web;
using Toki.ActivityPub.Emojis;
using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.DTO;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Renderers;

namespace Toki.ActivityPub.Formatters;

/// <summary>
/// The content formatting service.
/// </summary>
public class ContentFormatter(
    UserRepository repo,
    EmojiRepository emojiRepo,
    MicroformatsRenderer microformats)
{
    /// <summary>
    /// The regex for mentions.
    /// </summary>
    private readonly Regex _mentionRegex = new(@"[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?){1,500}", RegexOptions.Compiled);

    /// <summary>
    /// The regex for hashtags.
    /// </summary>
    private readonly Regex _hashtagRegex = new(@"\W(\#[a-zA-Z]+\b)");

    /// <summary>
    /// The regex for emojis.
    /// </summary>
    private readonly Regex _emojiRegex = new(@":([a-zA-Z0-9_\-]+):");
    
    /// <summary>
    /// Extracts the mentions from the content.
    /// </summary>
    /// <param name="content">The mentions.</param>
    /// <returns>The list of mentions.</returns>
    private async Task<IReadOnlyList<User>> ExtractMentions(string content)
    {
        var matches = _mentionRegex.Matches(content);
        var mentions = new List<User>();
        
        foreach (Match match in matches)
        {
            if (!match.Success)
                continue;

            var user = await repo.FindByHandle(match.Value);
            if (user is null)
                continue;
            
            mentions.Add(user);
        }
        
        return mentions;
    }

    /// <summary>
    /// Replaces all the mentions with the links.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="mentions">The mentions.</param>
    /// <returns>The resulting new string.</returns>
    private string ReplaceMentions(
        string content,
        IEnumerable<User> mentions)
    {
        var output = mentions.Aggregate(content, 
            (current, mention) => 
                current.Replace(
                    $"@{mention.Handle}", 
                    microformats.Mention(mention)));

        return output;
    }

    /// <summary>
    /// Extracts the hashtags from a post.
    /// </summary>
    /// <param name="content">The content of the post.</param>
    /// <returns>The hashtags.</returns>
    private List<string> ExtractHashtags(
        string content)
    {
        var matches = _hashtagRegex.Matches(content);
        var hashtags = new List<string>();
        
        foreach (Match match in matches)
        {
            if (!match.Success || match.Groups.Count < 2)
                continue;

            hashtags.Add(match.Groups[1].Value);
        }
        
        return hashtags;
    }
    
    /// <summary>
    /// Replaces all the hashtags with the links.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="hashtags">The hashtags.</param>
    /// <returns>The resulting new string.</returns>
    private string ReplaceHashtags(
        string content,
        IEnumerable<string> hashtags)
    {
        var output = hashtags.Aggregate(content, 
            (current, tag) => 
                current.Replace(
                    tag, 
                    microformats.Hashtag(tag)));

        return output;
    }

    /// <summary>
    /// Collects all of the emojis for this post.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task<List<string>> ExtractEmojis(
        string content)
    {
        var matches = _emojiRegex.Matches(content);

        var shortcodes = new List<string>();
        foreach (Match match in matches)
        {
            if (!match.Success)
                continue;

            shortcodes.Add(match.Value);
        }

        var emoji = await emojiRepo
            .FindManyByNameAndInstance(
                shortcodes,
                null);

        return emoji
            .Select(e => e.Id.ToString())
            .ToList();
    }

    /// <summary>
    /// Replaces newlines with br tags.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The replaced content.</returns>
    private string ReplaceNewlines(
        string content)
    {
        return content
            .ReplaceLineEndings("<br>");
    }
    
    /// <summary>
    /// Formats the content.
    /// </summary>
    /// <param name="content">Said content.</param>
    /// <returns>The formatting result.</returns>
    public async Task<ContentFormattingResult?> Format(string content)
    {
        // TODO: I'd love a kind of middleware type of pipeline here...
        var output = HttpUtility.HtmlEncode(content);
        
        var mentions = await ExtractMentions(output);
        output = ReplaceMentions(
            output,
            mentions);

        var tags = ExtractHashtags(output);
        output = ReplaceHashtags(
            output,
            tags);

        var emojis = await ExtractEmojis(output);
        output = ReplaceNewlines(output);
        
        return new ContentFormattingResult(
            output,
            mentions,
            tags,
            emojis);
    }
}