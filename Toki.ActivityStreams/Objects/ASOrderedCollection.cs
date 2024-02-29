using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ordered ActivityStreams collection.
/// </summary>
/// <typeparam name="TObject">The type of the object.</typeparam>
public class ASOrderedCollection<TObject> : ASCollection<TObject>
    where TObject : ASObject
{
    /// <summary>
    /// Constructs a new ordered collection.
    /// </summary>
    public ASOrderedCollection()
        : base("OrderedCollection")
    {
        
    }
    
    /// <summary>
    /// Constructs a ordered collection of a given type.
    /// </summary>
    /// <param name="type">The type.</param>
    protected ASOrderedCollection(string type = "OrderedCollection")
        : base(type)
    {
        
    }
    
    /// <summary>
    /// An ordered collection of items.
    /// </summary>
    [JsonPropertyName("orderedItems")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<TObject> OrderedItems { get; set; } = null!;
}