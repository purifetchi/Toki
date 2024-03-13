using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Toki.MastodonApi.Schemas.Requests.Statuses;

/// <summary>
/// A request to post a media attachment.
/// </summary>
public record PostMediaRequest
{
    /// <summary>
    /// The file to be attached, encoded using multipart form data. The file must have a MIME type.
    /// </summary>
    [BindProperty(Name = "file")]
    [JsonIgnore]
    public IFormFile? File { get; init; }
    
    /// <summary>
    /// The custom thumbnail of the media to be attached, encoded using multipart form data.
    /// </summary>
    [BindProperty(Name = "thumbnail")]
    [JsonIgnore]
    public IFormFile? Thumbnail { get; init; }
    
    /// <summary>
    /// A plain-text description of the media, for accessibility purposes.
    /// </summary>
    [BindProperty(Name = "description")]
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    
    /// <summary>
    /// Two floating points (x,y), comma-delimited, ranging from -1.0 to 1.0. 
    /// </summary>
    [BindProperty(Name = "focus")]
    [JsonPropertyName("focus")]
    public string? Focus { get; init; }
}