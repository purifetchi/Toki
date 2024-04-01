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
    /// Renders an <see cref="ASDelete"/> for a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The create activity.</returns>
    public ASDelete RenderDeletionForNote(Post post)
    {
        var linkedAuthor = userRenderer.RenderLinkedActorFrom(post.Author);
        var note = RenderLinkedNoteFrom(post);
        
        var delete = new ASDelete()
        {
            Id = $"{note.Id}/activity#delete",
            Actor = linkedAuthor,
            Object = note,
            
            PublishedAt = DateTimeOffset.UtcNow
        };

        return delete;
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
    
    /// <summary>
    /// Renders an <see cref="ASUndo"/> for a <see cref="ASLike"/> related to a post.
    /// </summary>
    /// <param name="likingUser">The liking user.</param>
    /// <param name="post">The post.</param>
    /// <returns>The like activity.</returns>
    public ASUndo RenderUndoLikeForNote(
        User likingUser,
        Post post)
    {
        var (to, cc) = GetToAndCcFor(post);
        var actorPath = pathRenderer.GetPathToActor(likingUser);
        var actorLink = userRenderer.RenderLinkedActorFrom(likingUser);
        
        var undo = new ASUndo()
        {
            // This ID doesn't really matter, as long as it's random lol
            Id = $"{actorPath}#undos/{Ulid.NewUlid()}",
            Actor = actorLink,

            Object = new ASLike()
            {
                Id = $"{actorPath}#likes/{post.Id}",
                Actor = actorLink,

                Object = RenderLinkedNoteFrom(post),
            
                PublishedAt = DateTimeOffset.UtcNow,
            
                To = to.Concat(cc)
                    .ToList()
            }
        };

        return undo;
    }
    
    /// <summary>
    /// Renders an <see cref="ASUndo"/> for a <see cref="ASAnnounce"/> related to a post.
    /// </summary>
    /// <param name="boostingUser">The boosting user.</param>
    /// <param name="post">The post.</param>
    /// <returns>The like activity.</returns>
    public ASUndo RenderUndoAnnounceForNote(
        User boostingUser,
        Post post)
    {
        var (to, cc) = GetToAndCcFor(post);
        var actorPath = pathRenderer.GetPathToActor(boostingUser);
        var actorLink = userRenderer.RenderLinkedActorFrom(boostingUser);
        
        var undo = new ASUndo()
        {
            // This ID doesn't really matter, as long as it's random lol
            Id = $"{actorPath}#undos/{Ulid.NewUlid()}",
            Actor = actorLink,

            Object = new ASAnnounce()
            {
                Id = $"{actorPath}#boosts/{post.Id}",
                Actor = actorLink,

                Object = RenderLinkedNoteFrom(post),
            
                PublishedAt = DateTimeOffset.UtcNow,
            
                To = to.Concat(cc)
                    .ToList()
            }
        };

        return undo;
    }

    /// <summary>
    /// Renders an <see cref="ASAdd"/> for a post.
    /// </summary>
    /// <param name="actor">The actor performing the add.</param>
    /// <param name="post">The added post.</param>
    /// <param name="target">The target collection (the featured posts by default).</param>
    /// <returns>The activity.</returns>
    public ASAdd RenderAddForPost(
        User actor,
        Post post,
        string? target = null)
    {
        var (to, cc) = GetToAndCcFor(post);
        var actorPath = pathRenderer.GetPathToActor(actor);
        var actorLink = userRenderer.RenderLinkedActorFrom(actor);

        var add = new ASAdd()
        {
            Id = $"{actorPath}#adds/{Ulid.NewUlid()}",
            Actor = actorLink,
            Object = RenderLinkedNoteFrom(post),
            
            Target = ASObject.Link(target ?? $"{actorPath}/collections/featured"),

            To = to.Concat(cc)
                .ToList()
        };

        return add;
    }
    
    /// <summary>
    /// Renders an <see cref="ASRemove"/> for a post.
    /// </summary>
    /// <param name="actor">The actor performing the remove.</param>
    /// <param name="post">The removed post.</param>
    /// <param name="target">The target collection (the featured posts by default).</param>
    /// <returns>The activity.</returns>
    public ASRemove RenderRemoveForPost(
        User actor,
        Post post,
        string? target = null)
    {
        var (to, cc) = GetToAndCcFor(post);
        var actorPath = pathRenderer.GetPathToActor(actor);
        var actorLink = userRenderer.RenderLinkedActorFrom(actor);

        var add = new ASRemove()
        {
            Id = $"{actorPath}#removes/{Ulid.NewUlid()}",
            Actor = actorLink,
            Object = RenderLinkedNoteFrom(post),
            
            Target = ASObject.Link(target ?? $"{actorPath}/collections/featured"),

            To = to.Concat(cc)
                .ToList()
        };

        return add;
    }
}