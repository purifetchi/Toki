using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// A specialization of <see cref="ASCollection{TObject}"/> denoting a single page from it.
/// </summary>
/// <typeparam name="TObject">The type of the object this collection contains.</typeparam>
public class ASCollectionPage<TObject> : ASCollection<TObject>
    where TObject : ASObject
{
    /// <summary>
    /// The previous page.
    /// </summary>
    [JsonPropertyName("prev")]
    public string? Previous { get; set; }
    
    /// <summary>
    /// The next page.
    /// </summary>
    [JsonPropertyName("next")]
    public string? Next { get; set; }
    
    /// <summary>
    /// The collection this page is a part of.
    /// </summary>
    [JsonPropertyName("partOf")]
    public string? PartOf { get; set; }
}