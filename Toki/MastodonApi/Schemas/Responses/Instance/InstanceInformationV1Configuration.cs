using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Responses.Instance;

/// <summary>
/// Mastodon api instance configuration information. 
/// </summary>
public class InstanceInformationV1Configuration
{
    /// <summary>
    /// The information related to statuses.
    /// </summary>
    [JsonPropertyName("statuses")]
    public InstanceInformationStatuses? Statuses { get; set; }
    
    // TODO: media_attachments, polls
}