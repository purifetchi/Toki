using Toki.ActivityPub.Models.Users;

namespace Toki.ActivityPub.Models.DTO;

/// <summary>
/// A request for a user update.
/// </summary>
public record UserUpdateRequest
{
    /// <summary>
    /// The new username.
    /// </summary>
    public string? DisplayName { get; init; }
    
    /// <summary>
    /// The new bio.
    /// </summary>
    public string? Bio { get; init; }
    
    /// <summary>
    /// The URL of the new avatar.
    /// </summary>
    public string? AvatarUrl { get; init; }
    
    /// <summary>
    /// Should we remove the avatar?
    ///
    /// TODO: Come up with a better idea.
    /// </summary>
    public bool RemoveAvatar { get; init; }
    
    /// <summary>
    /// The URL of the new header.
    /// </summary>
    public string? HeaderUrl { get; init; }
    
    /// <summary>
    /// Should we remove the header?
    ///
    /// TODO: Come up with a better idea.
    /// </summary>
    public bool RemoveHeader { get; init; }

    /// <summary>
    /// The new fields.
    /// </summary>
    public IReadOnlyList<UserProfileField>? Fields { get; init; }
    
    /// <summary>
    /// The new follow approval value.
    /// </summary>
    public bool? RequiresFollowApproval { get; init; }
}