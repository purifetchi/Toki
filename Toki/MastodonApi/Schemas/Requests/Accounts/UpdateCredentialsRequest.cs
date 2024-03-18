using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Toki.MastodonApi.Schemas.Requests.Accounts;

/// <summary>
/// The data for the credential updates.
/// </summary>
public record UpdateCredentialsRequest
{
    /// <summary>
    /// The display name to use for the profile.
    /// </summary>
    [BindProperty(Name = "display_name")]
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; init; }
    
    /// <summary>
    /// The account bio.
    /// </summary>
    [BindProperty(Name = "note")]
    [JsonPropertyName("note")]
    public string? Bio { get; init; }
    
    /// <summary>
    /// Avatar image encoded using multipart/form-data.
    /// </summary>
    [BindProperty(Name = "avatar")]
    [JsonIgnore]
    public IFormFile? Avatar { get; init; }
    
    /// <summary>
    /// Header image encoded using multipart/form-data.
    /// </summary>
    [BindProperty(Name = "header")]
    [JsonIgnore]
    public IFormFile? Header { get; init; }
    
    /// <summary>
    /// Whether manual approval of follow requests is required.
    /// </summary>
    [BindProperty(Name = "locked")]
    [JsonPropertyName("locked")]
    public bool? RequiresFollowApproval { get; init; }
    
    // TODO: The rest of the settings.
}