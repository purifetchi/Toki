using Toki.ActivityPub.Models;
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
    /// Renders an account from a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="renderCredentialAccount">Should we render a CredentialAccount?</param>
    /// <returns>The account.</returns>
    public Account RenderAccountFrom(
        User user,
        bool renderCredentialAccount = false)
    {
        return new Account
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
            
            Bio = user.Bio,

            Avatar = user.AvatarUrl,
            Header = user.BannerUrl,
            
            ManuallyApprovesRequests = user.RequiresFollowApproval,
            
            Source = renderCredentialAccount ? new CredentialAccountSource()
            {
                Note = user.Bio
            } : null,
            
            CreatedAt = user.CreatedAt
        };
    }
}