using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Responses.Instance;

/// <summary>
/// The information about media attachments for this instance.
/// </summary>
public record InstanceInformationMediaAttachments
{
    /// <summary>
    /// The supported MIME types.
    /// </summary>
    [JsonPropertyName("supported_mime_types")]
    public IReadOnlyList<string>? SupportedMimeTypes { get; init; } = [];
    
    /// <summary>
    /// The maximum size of any uploaded image, in bytes.
    /// </summary>
    [JsonPropertyName("image_size_limit")]
    public int ImageSizeLimit { get; init; }
    
    /// <summary>
    /// The maximum number of pixels (width times height) for image uploads.
    /// </summary>
    [JsonPropertyName("image_matrix_limit")]
    public int ImageMatrixLimit { get; init; }
    
    /// <summary>
    /// The maximum size of any uploaded video, in bytes.
    /// </summary>
    [JsonPropertyName("video_size_limit")]
    public int VideoSizeLimit { get; init; }
    
    /// <summary>
    /// The maximum number of pixels (width times height) for video uploads.
    /// </summary>
    [JsonPropertyName("video_matrix_limit")]
    public int VideoMatrixLimit { get; init; }
    
    /// <summary>
    /// The maximum frame rate for any uploaded video.
    /// </summary>
    [JsonPropertyName("video_frame_rate_limit")]
    public int VideoFrameRateLimit { get; init; }
}