namespace Toki.ActivityPub.Models.DTO;

/// <summary>
/// The relationship information between two users.
/// </summary>
/// <param name="Followed">Is this user followed?</param>
/// <param name="Following">Is this user following you?</param>
/// <param name="RequestedFollow">Did this user request a follow?</param>
/// <param name="RequestedBy">Were you requested by this user?</param>
public record RelationshipInformation(
    bool Followed,
    bool Following,
    bool RequestedFollow,
    bool RequestedBy);