using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// The source field for the CredentialAccount.
/// </summary>
public record CredentialAccountSource
{
    /// <summary>
    /// The default posting privacy.
    /// </summary>
    [JsonPropertyName("privacy")]
    public string? Privacy { get; set; } = "public";
    
    /// <summary>
    /// The bio, but in plaintext.
    /// </summary>
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// The fields.
    /// </summary>
    [JsonPropertyName("fields")]
    public List<string>? Fields { get; set; } = []; // TODO: This is a polyfill.
    
    /// <summary>
    /// Should new posts be automatically marked as sensitive?
    /// </summary>
    [JsonPropertyName("sensitive")]
    public bool MarkSensitiveAsDefault { get; set; }

    /// <summary>
    /// The default posting language.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; } = "en";
    
    /// <summary>
    /// The amount of pending follow requests.
    /// </summary>
    [JsonPropertyName("follow_requests_count")]
    public int PendingFollowRequests { get; set; }
}