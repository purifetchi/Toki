using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// Represents a notification of an event relevant to the user.
/// </summary>
public record Notification
{
    /// <summary>
    /// The id of the notification in the database.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    /// <summary>
    /// The type of event that resulted in the notification.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }
    
    /// <summary>
    /// The timestamp of the notification.
    /// </summary>
    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>
    /// The account that performed the action that generated the notification.
    /// </summary>
    [JsonPropertyName("account")]
    public required Account Account { get; init; }
    
    /// <summary>
    /// Status that was the object of the notification. Attached when the type of the notification is favourite, reblog, status, mention, poll, or update.
    /// </summary>
    [JsonPropertyName("status")]
    public Status? Status { get; init; }
    
    // TODO: Report, Relationship_Severance_Event
}