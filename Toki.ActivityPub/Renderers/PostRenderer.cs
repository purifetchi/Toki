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
    /// Gets the To/CC tuple for a given post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The resulting tuple.</returns>
    private (IReadOnlyList<string>, IReadOnlyList<string>) GetToAndCcFor(Post post)
    {
        const string asPublic = "https://www.w3.org/ns/activitystreams#Public";
        
        var to = post.Visibility switch
        {
            PostVisibility.Public => [asPublic],
            PostVisibility.Unlisted or PostVisibility.Followers => [$"{pathRenderer.GetPathToActor(post.Author)}/followers"],
            _ => new List<string>()
        };
        
        var cc = post.Visibility switch
        {
            PostVisibility.Public => [$"{pathRenderer.GetPathToActor(post.Author)}/followers"],
            PostVisibility.Unlisted => [asPublic],
            _ => new List<string>()
        };
        
        // TODO: CC should also have people that were mentioned. Do that when we support mentions.

        return (to, cc);
    }

    /// <summary>
    /// Renders the attachments for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The attachments.</returns>
    private IReadOnlyList<ASDocument>? RenderAttachmentsFrom(
        Post post)
    {
        return post.Attachments?
            .Select(attach => new ASDocument
            {
                Type = "Document",
                Name = attach.Description,
                MediaType = attach.Mime,
                Url = attach.Url,
            })
            .ToList();
    }

    /// <summary>
    /// Renders the mentions for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The mentions.</returns>
    private IReadOnlyList<ASLink>? RenderMentionsFrom(
        Post post)
    {
        return post.UserMentions?
            .Select(mention => new ASMention()
            {
                Type = "Mention",
                Name = $"@{mention.Handle}",
                Href = mention.Url
            })
            .ToList();
    }
    
    /// <summary>
    /// Renders an <see cref="ASNote"/> from a <see cref="Post"/>
    /// </summary>
    /// <param name="post">The post to render.</param>
    /// <returns>The ASNote.</returns>
    public ASNote RenderFullNoteFrom(Post post)
    {
        var (to, cc) = GetToAndCcFor(post);        
        
        return new ASNote()
        {
            Context = null,
            
            Id = RenderLinkedNoteFrom(post).Id,
            AttributedTo = userRenderer.RenderLinkedActorFrom(post.Author),

            InReplyTo = post.Parent is not null ?
                ASObject.Link(
                    post.Parent.RemoteId ?? pathRenderer.GetPathToPost(post.Parent)) :
                null,
            
            Content = post.Content,
            Sensitive = post.Sensitive,
            
            To = to,
            Cc = cc,
            
            Attachments = RenderAttachmentsFrom(post),
            Tags = RenderMentionsFrom(post),
            PublishedAt = post.CreatedAt
        };
    }

    /// <summary>
    /// Renders a linked <see cref="ASObject"/> for a note.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The resulting linked object.</returns>
    public ASObject RenderLinkedNoteFrom(Post post)
    {
        return ASObject.Link(
            post.RemoteId ?? pathRenderer.GetPathToPost(post));
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
            
            PublishedAt = DateTimeOffset.UtcNow
        };

        return create;
    }

    /// <summary>
    /// Renders an <see cref="ASAnnounce"/> for boosting a post.
    /// </summary>
    /// <param name="booster">The boosting user.</param>
    /// <param name="post">The post.</param>
    /// <returns>The announce activity.</returns>
    public ASAnnounce RenderBoostForNote(
        User booster,
        Post post)
    {
        var (to, cc) = GetToAndCcFor(post);
        
        var announce = new ASAnnounce()
        {
            Id = $"{pathRenderer.GetPathToActor(booster)}#boosts/{post.Id}",
            Actor = userRenderer.RenderLinkedActorFrom(booster),

            Object = RenderLinkedNoteFrom(post),
            
            PublishedAt = DateTimeOffset.UtcNow,
            
            To = to.Concat(cc)
                .ToList()
        };

        return announce;
    }
    
    /// <summary>
    /// Renders an <see cref="ASLike"/> for liking a post.
    /// </summary>
    /// <param name="likingUser">The liking user.</param>
    /// <param name="post">The post.</param>
    /// <returns>The like activity.</returns>
    public ASLike RenderLikeForNote(
        User likingUser,
        Post post)
    {
        var (to, cc) = GetToAndCcFor(post);
        
        var like = new ASLike()
        {
            Id = $"{pathRenderer.GetPathToActor(likingUser)}#likes/{post.Id}",
            Actor = userRenderer.RenderLinkedActorFrom(likingUser),

            Object = RenderLinkedNoteFrom(post),
            
            PublishedAt = DateTimeOffset.UtcNow,
            
            To = to.Concat(cc)
                .ToList()
        };

        return like;
    }
}