using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Renderers;

/// <summary>
/// The renderer class for Posts.
/// </summary>
/// <param name="pathRenderer">The instance path renderer.</param>
public class PostRenderer(
    InstancePathRenderer pathRenderer,
    UserRenderer userRenderer)
{
    /// <summary>
    /// Renders an <see cref="ASNote"/> from a <see cref="Post"/>
    /// </summary>
    /// <param name="post">The post to render.</param>
    /// <returns>The ASNote.</returns>
    public ASNote RenderFullNoteFrom(Post post)
    {
        // FIXME: Unlisted is still broken on Pleroma despite looking correct.
        var to = post.Visibility switch
        {
            PostVisibility.Public => ["https://www.w3.org/ns/activitystreams#Public"],
            PostVisibility.Unlisted => [$"{pathRenderer.GetPathToActor(post.Author)}/followers"],
            _ => new List<string>()
        };
        
        var cc = post.Visibility switch
        {
            PostVisibility.Public or PostVisibility.Followers => [$"{pathRenderer.GetPathToActor(post.Author)}/followers"],
            PostVisibility.Unlisted => ["https://www.w3.org/ns/activitystreams#Public"],
            _ => new List<string>()
        };
        
        // TODO: CC should also have people that were mentioned. Do that when we support mentions.
        
        return new ASNote()
        {
            Context = null,
            
            Id = post.RemoteId ?? pathRenderer.GetPathToPost(post),
            AttributedTo = userRenderer.RenderLinkedActorFrom(post.Author),

            InReplyTo = post.Parent is not null ?
                ASObject.Link(
                    post.Parent.RemoteId ?? pathRenderer.GetPathToPost(post.Parent)) :
                null,
            
            Content = post.Content,
            Sensitive = post.Sensitive,
            
            To = to,
            Cc = cc,
            
            PublishedAt = ((DateTimeOffset)post.CreatedAt).ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z"
        };
    }

    /// <summary>
    /// Renders an <see cref="ASCreate"/> for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The create activity.</returns>
    public ASCreate RenderCreationForNote(Post post)
    {
        var linkedAuthor = userRenderer.RenderLinkedActorFrom(post.Author);
        var note = RenderFullNoteFrom(post);
        var create = new ASCreate()
        {
            Id = $"{note.Id}/activity",
            Actor = linkedAuthor,
            Object = note,
            
            To = note.To,
            Cc = note.Cc,
            
            PublishedAt = DateTimeOffset.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z"
        };

        return create;
    }
}