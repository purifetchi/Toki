using System.Text.Json.Serialization;
using Toki.MastodonApi.Schemas.Objects;

namespace Toki.MastodonApi.Schemas.Responses.Instance;

/// <summary>
/// The v1 version of the instance information response.
/// </summary>
public record InstanceInformationV1Response
{
    /// <summary>
    /// The uri.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; init; }
    
    /// <summary>
    /// The title of the server.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }
    
    /// <summary>
    /// The short description of the server.
    /// </summary>
    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; init; }
    
    /// <summary>
    /// The description of the server.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    
    /// <summary>
    /// The version of the software.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }
    
    /// <summary>
    /// The version of the software.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>
    /// The version of the software.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; init; } = null;

    /// <summary>
    /// The languages supported by this server.
    /// </summary>
    [JsonPropertyName("languages")]
    public IReadOnlyList<string> Languages { get; init; } = ["en"];

    /// <summary>
    /// An itemized list of rules for this website.
    /// </summary>
    [JsonPropertyName("rules")]
    public IReadOnlyList<OrderedTextItem> Rules { get; init; } = [];
    
    /// <summary>
    /// URLs of interest for clients apps.
    /// </summary>
    [JsonPropertyName("urls")]
    public InstanceInformationUrls Urls { get; init; } = new();

    /// <summary>
    /// The instance configuration.
    /// </summary>
    [JsonPropertyName("configuration")]
    public InstanceInformationV1Configuration? Configuration { get; init; }
    
    /// <summary>
    /// Statistics about how much information the instance contains.
    /// </summary>
    [JsonPropertyName("stats")]
    public InstanceInformationStatistics? Statistics { get; init; }
    
    /// <summary>
    /// Are the registrations open?
    /// </summary>
    [JsonPropertyName("registrations")]
    public bool RegistrationsOpen { get; init; } = false;
    
    /// <summary>
    /// Is approval required for new accounts?
    /// </summary>
    [JsonPropertyName("approval_required")]
    public bool ApprovalRequired { get; init; } = false;
    
    /// <summary>
    /// Are invites enabled for this instance?
    /// </summary>
    [JsonPropertyName("invites_enabled")]
    public bool InvitesEnabled { get; init; } = false;
    
    // TODO: Rules, contact account. stats
}