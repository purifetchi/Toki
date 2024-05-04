using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams document.
/// </summary>
public class ASDocument : ASLink
{
    /// <summary>
    /// The url of the document.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    /// <summary>
    /// The name of the document.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// The hyperlink.
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; set; }
    
    /// <summary>
    /// The media type of the document.
    /// </summary>
    [JsonPropertyName("mediaType")]
    public string? MediaType { get; set; }
}