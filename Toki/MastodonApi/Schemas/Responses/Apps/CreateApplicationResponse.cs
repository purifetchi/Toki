using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Responses.Apps;

/// <summary>
/// The response for when the server has registered an oauth2 application.
/// </summary>
public record CreateApplicationResponse
{
    /// <summary>
    /// The id.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    /// <summary>
    /// The name of the client.
    /// </summary>
    [JsonPropertyName("name")]
    public required string ClientName { get; init; }
    
    /// <summary>
    /// The id of the client app.
    /// </summary>
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }
    
    /// <summary>
    /// The id of the client app.
    /// </summary>
    [JsonPropertyName("client_secret")]
    public required string ClientSecret { get; init; }
    
    /// <summary>
    /// The URIs which are a valid redirect target.
    /// </summary>
    [JsonPropertyName("redirect_uri")]
    public required string RedirectUri { get; init; }
    
    /// <summary>
    /// The website.
    /// </summary>
    [JsonPropertyName("website")]
    public string? Website { get; init; }
}