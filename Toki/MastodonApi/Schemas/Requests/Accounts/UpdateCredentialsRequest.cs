using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Toki.MastodonApi.Schemas.Objects;

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
    
    /// <summary>
    /// The profile fields to be set.
    /// </summary>
    [BindProperty(Name = "fields_attributes")]
    public IReadOnlyList<Field>? FieldAttributes { get; init; }
    
    /// <summary>
    /// The profile fields to be set (but JSON).
    /// </summary>
    // NOTE: Horrible hack, the way binding works in ASP.NET Core pretty much requires us to specify two
    //       different objects, as when it's a form, it's treated like a list of fields, but when it's in JSON
    //       it's a dictionary from string to the field model.
    [JsonPropertyName("fields_attributes")]
    public IReadOnlyDictionary<string, Field>? FieldAttributesJsonDictionary { get; init; }

    /// <summary>
    /// Gets the profile fields, aware whether we're using the form or the JSON variant.
    /// </summary>
    /// <returns>The fields.</returns>
    public IEnumerable<Field>? GetFields() =>
        FieldAttributes ?? FieldAttributesJsonDictionary?.Values;
    
    // TODO: The rest of the settings.
}