using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// A specialization of <see cref="ASOrderedCollection{TObject}"/> denoting a single page from it.
/// </summary>
/// <typeparam name="TObject">The type of the object this collection contains.</typeparam>
// NOTE: This should've been a specialization of ASCollection<TObject> that also
//       inherits from ASOrderedCollection<TObject> (as specified by the AS2.0 spec)
//       but because C# doesn't support multiple inheritance, I have to reimplement it.s
public class ASOrderedCollectionPage<TObject> : ASOrderedCollection<TObject>
    where TObject : ASObject
{
    /// <summary>
    /// Constructs an ordered collection page.
    /// </summary>
    public ASOrderedCollectionPage()
        : base("OrderedCollectionPage")
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