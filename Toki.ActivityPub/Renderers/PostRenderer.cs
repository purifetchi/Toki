using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.Enums;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityStreams.Activities;
using Toki.ActivityStreams.Context;
using Toki.ActivityStreams.Objects;

namespace Toki.ActivityPub.Renderers;

/// <summary>
/// The renderer class for Posts.
/// </summary>
/// <param name="pathRenderer">The instance path renderer.</param>
public class PostRenderer(
    InstancePathRenderer pathRenderer,
    EmojiRepository emojiRepo,
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
    /// Gets the To/CC tuple for an action that an actor has performed on a post.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <param name="actor">The actor.</param>
    /// <returns>The tuple.</returns>
    private (IReadOnlyList<string>, IReadOnlyList<string>) GetToAndCcForAction(
        Post post,
        User actor)
    {
        const string asPublic = "https://www.w3.org/ns/activitystreams#Public";
        
        var to = post.Visibility switch
        {
            PostVisibility.Public or PostVisibility.Unlisted => [
                post.Author.RemoteId ?? pathRenderer.GetPathToActor(post.Author),
                $"{pathRenderer.GetPathToActor(actor)}/followers"
            ],
            PostVisibility.Followers or PostVisibility.Direct => [
                post.Author.RemoteId ?? pathRenderer.GetPathToActor(post.Author)
                // TODO: We should also have the mentioned people here.
            ],
            _ => new List<string>()
        };

        var cc = post.Visibility switch
        {
            PostVisibility.Public => [asPublic],
            _ => new List<string>()
        };

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
    /// Renders the hashtags.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The hashtags.</returns>
    private IReadOnlyList<ASLink>? RenderHashtagsFrom(
        Post post)
    {
        return post.Tags?
            .Select(tag => new ASHashtag
            {
                Type = "Hashtag",
                Name = tag,
                Href = pathRenderer.GetPathToHashtag(tag)
            })
            .ToList();
    }

    /// <summary>
    /// Renders the emojis.
    /// </summary>
    /// <param name="post">The post.</param>
    /// <returns>The emojis.</returns>
    private async Task<IReadOnlyList<ASEmoji>?> RenderEmojiFrom(
        Post post)
    {
        if (post.Emojis is null || post.Emojis.Count < 1)
            return null;

        var emoji = await emojiRepo.FindManyByIds(
            post.Emojis
                .Select(Ulid.Parse)
                .ToList()
            );

        return emoji.Select(
                e => new ASEmoji
                {
                    Type = "Emoji",
                    Id = e.RemoteUrl,
                    Icon = new ASImage
                    {
                        Type = "Image",
                        Url = e.RemoteUrl
                    },
                    Name = e.Shortcode
                })
            .ToList();
    }
    
    /// <summary>
    /// Renders an <see cref="ASNote"/> from a <see cref="Post"/>
    /// </summary>
    /// <param name="post">The post to render.</param>
    /// <param name="includeContext">Whether to include the context.</param>
    /// <returns>The ASNote.</returns>
    public async Task<ASNote> RenderFullNoteFrom(
        Post post,
        bool includeContext = false)
    {
        var (to, cc) = GetToAndCcFor(post);

        var tags = RenderMentionsFrom(post)?
            .Concat(RenderHashtagsFrom(post) ?? [])
            .Concat(await RenderEmojiFrom(post) ?? [])
            .ToList();
        
        return new ASNote()
        {
            Context = includeContext ? 
                LdContext.Default :
                null,
            
            Id = RenderLinkedNoteFrom(post).Id,
            AttributedTo = userRenderer.RenderLinkedActorFrom(post.Author),

            InReplyTo = post.Parent is not null ?
                ASObject.Link(
                    post.Parent.RemoteId ?? pathRenderer.GetPathToPost(post.Parent)) :
                null,
            
            Content = post.Content,
            Sensitive = post.Sensitive,
            Summary = post.ContentWarning,
            
            To = to,
            Cc = cc,
            
            Attachments = RenderAttachmentsFrom(post),
            Tags = tags,
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
    public async Task<ASCreate> RenderCreationForNote(Post post)
    {
        var linkedAuthor = userRenderer.RenderLinkedActorFrom(post.Author);
        var note = await RenderFullNoteFrom(post);
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
        var (to, cc) = GetToAndCcForAction(
            post,
            booster);
        
        var announce = new ASAnnounce()
        {
            Id = $"{pathRenderer.GetPathToActor(booster)}#boosts/{post.Id}",
            Actor = userRenderer.RenderLinkedActorFrom(booster),

            Object = RenderLinkedNoteFrom(post),
            
            PublishedAt = DateTimeOffset.UtcNow,
            
            To = to,
            Cc = cc
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
        var (to, cc) = GetToAndCcForAction(
            post,
            likingUser);
        
        var like = new ASLike()
        {
            Id = $"{pathRenderer.GetPathToActor(likingUser)}#likes/{post.Id}",
            Actor = userRenderer.RenderLinkedActorFrom(likingUser),

            Object = RenderLinkedNoteFrom(post),
            
            PublishedAt = DateTimeOffset.UtcNow,
            
            To = to,
            Cc = cc
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
        var (to, cc) = GetToAndCcForAction(
            post,
            likingUser);
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
            
                To = to,
                Cc = cc
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
        var (to, cc) = GetToAndCcForAction(
            post,
            boostingUser);
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
            
                To = to,
                Cc = cc
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
        var (to, cc) = GetToAndCcForAction(
            post,
            actor);
        var actorPath = pathRenderer.GetPathToActor(actor);
        var actorLink = userRenderer.RenderLinkedActorFrom(actor);

        var add = new ASAdd()
        {
            Id = $"{actorPath}#adds/{Ulid.NewUlid()}",
            Actor = actorLink,
            Object = RenderLinkedNoteFrom(post),
            
            Target = ASObject.Link(target ?? $"{actorPath}/collections/featured"),

            To = to,
            Cc = cc
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
        var (to, cc) = GetToAndCcForAction(
            post,
            actor);
        var actorPath = pathRenderer.GetPathToActor(actor);
        var actorLink = userRenderer.RenderLinkedActorFrom(actor);

        var add = new ASRemove()
        {
            Id = $"{actorPath}#removes/{Ulid.NewUlid()}",
            Actor = actorLink,
            Object = RenderLinkedNoteFrom(post),
            
            Target = ASObject.Link(target ?? $"{actorPath}/collections/featured"),

            To = to,
            Cc = cc
        };

        return add;
    }
}