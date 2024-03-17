using System.Text.RegularExpressions;
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
    InstancePathRenderer pathRenderer)
{
    /// <summary>
    /// The regex for mentions.
    /// </summary>
    private readonly Regex _mentionRegex = new(@"[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?){1,500}", RegexOptions.Compiled);
    
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
        // NOTE: The format of the <a> tag has been taken from Pleroma and seems to be working
        //       across multiple frontends.
        const string format = """<a href="{0}" class="u-url mention" data-user="{1}">@{2}</a>""";

        var output = mentions.Aggregate(content, 
            (current, mention) => 
                current.Replace(
                    $"@{mention.Handle}", 
                    string.Format(
                        format, 
                        mention.RemoteId ?? pathRenderer.GetPathToActor(mention), 
                        mention.Id,
                        mention.Handle)));

        return output;
    }
    
    /// <summary>
    /// Formats the content.
    /// </summary>
    /// <param name="content">Said content.</param>
    /// <returns>The formatting result.</returns>
    public async Task<ContentFormattingResult?> Format(string content)
    {
        // TODO: I'd love a kind of middleware type of pipeline here...
        var output = content;
        
        var mentions = await ExtractMentions(output);
        output = ReplaceMentions(
            content,
            mentions);
        
        return new ContentFormattingResult(
            output,
            mentions);
    }
}