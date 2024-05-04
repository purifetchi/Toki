using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams video.
/// </summary>
public class ASVideo : ASObject
{
    /// <summary>
    /// Creates a new ASVideo.
    /// </summary>
    public ASVideo()
        : base("Video")
    {
        
    }
    
    /// <summary>
    /// The title of the video.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// The description of the video.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    /// <summary>
    /// Who is this video attributed to?
    /// </summary>
    [JsonPropertyName("attributedTo")]
    public IReadOnlyList<ASObject>? AttributedTo { get; set; }
    
    /// <summary>
    /// The URLs associated with this video.
    /// </summary>
    [JsonPropertyName("url")]
    public IReadOnlyList<ASDocument>? Url { get; set; }
    
    /// <summary>
    /// The primary recipients of this video.
    /// </summary>
    [JsonPropertyName("to")]
    [JsonConverter(typeof(ListOrObjectConverter<string>))]
    public IReadOnlyList<string>? To { get; set; }
    
    /// <summary>
    /// The secondary recipients of this video.
    /// </summary>
    [JsonPropertyName("cc")]
    [JsonConverter(typeof(ListOrObjectConverter<string>))]
    public IReadOnlyList<string>? Cc { get; set; }
}