using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// Represents the relationship between accounts, such as following / blocking / muting / etc.
/// </summary>
public record Relationship
{
    /// <summary>
    /// The id.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    /// <summary>
    /// Are you following this user?
    /// </summary>
    [JsonPropertyName("following")]
    public bool Following { get; init; }
    
    /// <summary>
    /// Are you followed by this user?
    /// </summary>
    [JsonPropertyName("followed_by")]
    public bool FollowedBy { get; init; }

    /// <summary>
    /// Are you receiving this user’s boosts in your home timeline?
    /// </summary>
    [JsonPropertyName("showing_reblogs")]
    public bool ShowingReblogs { get; init; } = true;
    
    /// <summary>
    /// Have you enabled notifications for this user?
    /// </summary>
    [JsonPropertyName("notifying")]
    public bool ReceivesNotifications { get; init; }

    /// <summary>
    /// Which languages are you following from this user?
    /// </summary>
    [JsonPropertyName("languages")]
    public IReadOnlyList<string> Languages { get; init; } = ["en"];
    
    /// <summary>
    /// Are you blocking this user?
    /// </summary>
    [JsonPropertyName("blocking")]
    public bool Blocking { get; init; }
    
    /// <summary>
    /// Is this user blocking you?
    /// </summary>
    [JsonPropertyName("blocked_by")]
    public bool BlockedBy { get; init; }
    
    /// <summary>
    /// Are you muting this user?
    /// </summary>
    [JsonPropertyName("muting")]
    public bool Muting { get; init; }
    
    /// <summary>
    /// Are you muting notifications from this user?
    /// </summary>
    [JsonPropertyName("muting_notifications")]
    public bool MutingNotifications { get; init; }
    
    /// <summary>
    /// Do you have a pending follow request for this user?
    /// </summary>
    [JsonPropertyName("requested")]
    public bool RequestedFollow { get; init; }
    
    /// <summary>
    /// Has this user requested to follow you?
    /// </summary>
    [JsonPropertyName("requested_by")]
    public bool RequestedFollowBy { get; init; }
    
    /// <summary>
    /// Are you blocking this user’s domain?
    /// </summary>
    [JsonPropertyName("domain_blocking")]
    public bool DomainBlocking { get; init; }
    
    /// <summary>
    /// Are you featuring this user on your profile?
    /// </summary>
    [JsonPropertyName("endorsed")]
    public bool Endorsed { get; init; }

    /// <summary>
    /// This user’s profile bio.
    /// </summary>
    [JsonPropertyName("note")]
    public string Note { get; init; } = "";
}