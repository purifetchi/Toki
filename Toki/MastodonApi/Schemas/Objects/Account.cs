using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Objects;

/// <summary>
/// A Mastodon account.
/// </summary>
public record Account
{
    /// <summary>
    /// The id of the account.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    
    /// <summary>
    /// The username.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }
    
    /// <summary>
    /// The account webfinger acct uri.
    /// </summary>
    [JsonPropertyName("acct")]
    public string? WebFingerAcct { get; init; }
    
    /// <summary>
    /// The account's profile page.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }
    
    /// <summary>
    /// The account's display name.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; init; }
    
    /// <summary>
    /// The account's bio.
    /// </summary>
    [JsonPropertyName("note")]
    public string? Bio { get; init; }
    
    /// <summary>
    /// The account's avatar.
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; init; }
    
    /// <summary>
    /// The account's static avatar.
    /// </summary>
    [JsonPropertyName("avatar_static")]
    public string? AvatarStatic { get; init; }
    
    /// <summary>
    /// The account's header.
    /// </summary>
    [JsonPropertyName("header")]
    public string? Header { get; init; }
    
    /// <summary>
    /// The account's static header.
    /// </summary>
    [JsonPropertyName("header_static")]
    public string? HeaderStatic { get; init; }

    /// <summary>
    /// Whether this account manually approves requests.
    /// </summary>
    [JsonPropertyName("locked")]
    public bool ManuallyApprovesRequests { get; init; }
    
    /// <summary>
    /// When was this account created at?
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>
    /// The amount of statuses this account has posted.
    /// </summary>
    [JsonPropertyName("statuses_count")]
    public int StatusesCount { get; init; }
    
    /// <summary>
    /// The amount of statuses this account has posted.
    /// </summary>
    [JsonPropertyName("followers_count")]
    public int FollowersCount { get; init; }
    
    /// <summary>
    /// The amount of statuses this account has posted.
    /// </summary>
    [JsonPropertyName("following_count")]
    public int FollowingCount { get; init; }
    
    /// <summary>
    /// The extra information only visible to the local user.
    /// </summary>
    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CredentialAccountSource? Source { get; init; }
    
    /// <summary>
    /// The emojis in this account.
    /// </summary>
    [JsonPropertyName("emojis")]
    public IReadOnlyList<CustomEmoji> Emojis { get; init; } = [];
    
    /// <summary>
    /// The fields for this account.
    /// </summary>
    [JsonPropertyName("fields")]
    public IReadOnlyList<Field> Fields { get; init; } = [];

    [JsonPropertyName("bot")] public bool bot { get; init; } = false;
    [JsonPropertyName("group")] public bool group { get; init; } = false;
    [JsonPropertyName("discoverable")] public bool discoverable { get; init; } = false;
    [JsonPropertyName("noindex")] public bool noindex { get; init; } = false;
    [JsonPropertyName("moved")] public Account? moved { get; init; } = null;
    [JsonPropertyName("suspended")] public bool? suspended  { get; init; } = null;
    [JsonPropertyName("last_status_at")] public DateTimeOffset? last_status_at  { get; init; } = null;


}