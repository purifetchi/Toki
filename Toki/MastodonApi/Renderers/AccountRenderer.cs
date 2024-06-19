using Toki.ActivityPub.Models;
using Toki.ActivityPub.Models.DTO;
using Toki.ActivityPub.Renderers;
using Toki.MastodonApi.Schemas.Objects;

namespace Toki.MastodonApi.Renderers;

/// <summary>
/// The account renderer for Mastodon's API.
/// </summary>
public class AccountRenderer(
    InstancePathRenderer pathRenderer)
{
    /// <summary>
    /// A cache for already rendered users.
    /// </summary>
    private readonly Dictionary<Ulid, Account> _cache = new();
    
    /// <summary>
    /// Renders an account from a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="renderCredentialAccount">Should we render a CredentialAccount?</param>
    /// <returns>The account.</returns>
    public Account RenderAccountFrom(
        User user,
        bool renderCredentialAccount = false)
    {
        if (_cache.TryGetValue(user.Id, out var cachedAccount))
            return cachedAccount;
        
        var acc = new Account
        {
            Id = $"{user.Id}",
           
            Username = user.Handle
                .Split('@')
                .First(),
            
            WebFingerAcct = user.Handle,
            DisplayName = user.DisplayName,
            
            Url = user.IsRemote ? 
                user.RemoteId :
                pathRenderer.GetPathToActor(user),
            
            Bio = user.Bio ?? "",

            Avatar = user.AvatarUrl ?? 
                     pathRenderer.GetPathToDefaultAvatar(),
            AvatarStatic = user.AvatarUrl ?? 
                           pathRenderer.GetPathToDefaultAvatar(),
            
            Header = user.BannerUrl ?? 
                     pathRenderer.GetPathToDefaultBanner(),
            HeaderStatic = user.BannerUrl ?? 
                           pathRenderer.GetPathToDefaultBanner(),
            
            ManuallyApprovesRequests = user.RequiresFollowApproval,
            
            Source = renderCredentialAccount ? new CredentialAccountSource()
            {
                Note = user.Bio
            } : null,
            
            CreatedAt = user.CreatedAt,
            
            FollowingCount = user.FollowingCount,
            FollowersCount = user.FollowerCount,
            StatusesCount = user.PostCount,
            
            // TODO: These have the same layout, should we really do this whole select charade?
            Fields = user.Fields?
                .Select(f => new Field() 
                {
                    Name = f.Name ?? "",
                    Value = f.Value ?? "",
                    VerifiedAt = f.VerifiedAt
                }).ToList() ?? []
        };

        _cache[user.Id] = acc;
        return acc;
    }

    /// <summary>
    /// Renders a Mastodon <see cref="Relationship"/> from a Toki <see cref="RelationshipInformation"/>.
    /// </summary>
    /// <param name="target">The target user.</param>
    /// <param name="info">The relationship info.</param>
    /// <returns>The relationship.</returns>
    public Relationship RenderRelationshipFrom(
        User target, 
        RelationshipInformation info) => new Relationship
    {
        Id = target.Id.ToString(),
        Following = info.Followed,
        FollowedBy = info.Following,
        
        RequestedFollow = info.RequestedFollow,
        RequestedFollowBy = info.RequestedBy
    };
}