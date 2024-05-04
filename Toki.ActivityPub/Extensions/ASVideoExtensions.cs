using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Extensions;

/// <summary>
/// Extensions for the <see cref="ASVideo"/> class.
/// </summary>
public static class ASVideoExtensions
{
    /// <summary>
    /// Converts an ASVideo to an equivalent ASNote.
    /// </summary>
    /// <param name="video">The video.</param>
    /// <returns>The resulting ASNote.</returns>
    public static ASNote MockASNote(this ASVideo video)
    {
        const string htmlType = "text/html";
        const string videoType = "video/mp4";

        var playbackLink = video.Url?
            .FirstOrDefault(l => l.MediaType == htmlType)?
            .Href ?? video.Id;

        var videoLink = video.Url?
            .FirstOrDefault(l => l.MediaType == videoType);

        var content = $"""<a href="{playbackLink}" rel="ugc">{video.Name}</a><br><br>{video.Content}""";
        
        ASDocument? attachment = null;
        if (videoLink != null)
        {
            attachment = new ASDocument
            {
                Type = "Document",
                Url = videoLink.Href,
                MediaType = videoLink.MediaType
            };
        }

        // The "Group" actor takes priority in PeerTube.
        var attributedTo = video.AttributedTo?
            .FirstOrDefault(obj => obj.Type == "Group") ?? video.AttributedTo?.FirstOrDefault();
        
        return new ASNote
        {
            Id = video.Id,
            
            AttributedTo = attributedTo,
            Content = content,
            
            To = video.To,
            Cc = video.Cc,
            
            Attachments = [attachment],
            PublishedAt = video.PublishedAt
        };
    }
}