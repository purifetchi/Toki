using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams collection.
/// </summary>
/// <typeparam name="TObject">The type of the object.</typeparam>
public class ASCollection<TObject> : ASObject
    where TObject: ASObject
{
    /// <summary>
    /// Constructs a new collection.
    /// </summary>
    public ASCollection()
        : base("Collection")
    {
        
    }
    
    /// <summary>
    /// Constructs a collection of a given type.
    /// </summary>
    /// <param name="type">The type.</param>
    protected ASCollection(string type = "Collection")
        : base(type)
    {
        
    }
    
    /// <summary>
    /// The total amount of items.
    /// </summary>
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    /// <summary>
    /// An unordered collection of items.
    /// </summary>
    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<TObject> Items { get; set; } = null!;
    
    /// <summary>
    /// The first page of the collection.
    /// </summary>
    [JsonPropertyName("first")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ASObject? First { get; set; }
}