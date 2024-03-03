using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Toki.MastodonApi.Schemas.Errors;

/// <summary>
/// A generic API error response.
/// </summary>
public record MastodonApiError
{
    /// <summary>
    /// Constructs a new API error.
    /// </summary>
    /// <param name="msg">The message.</param>
    [SetsRequiredMembers]
    public MastodonApiError(string msg)
    {
        ErrorMessage = msg;    
    }
    
    /// <summary>
    /// The error message.
    /// </summary>
    [JsonPropertyName("error")]
    public required string ErrorMessage { get; init; }
}