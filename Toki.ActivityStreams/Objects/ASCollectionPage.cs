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
    /// Constructs a collection page.
    /// </summary>
    public ASCollectionPage()
        : base("CollectionPage")
    {
        
    }
    
    /// <summary>
    /// The previous page.
    /// </summary>
    [JsonPropertyName("prev")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Previous { get; set; }
    
    /// <summary>
    /// The next page.
    /// </summary>
    [JsonPropertyName("next")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Next { get; set; }
    
    /// <summary>
    /// The collection this page is a part of.
    /// </summary>
    [JsonPropertyName("partOf")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PartOf { get; set; }
}