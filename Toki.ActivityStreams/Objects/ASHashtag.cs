using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams hashtag.
/// </summary>
public class ASHashtag : ASLink
{
    /// <summary>
    /// The name of this hashtag.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// The link to the hashtag page.
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; set; }
}