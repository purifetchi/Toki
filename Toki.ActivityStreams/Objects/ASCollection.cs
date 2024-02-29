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
    /// The total amount of items.
    /// </summary>
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    /// <summary>
    /// An unordered collection of items.
    /// </summary>
    [JsonPropertyName("items")]
    public IList<TObject> Items { get; set; } = null!;
    
    /// <summary>
    /// The first page of the collection.
    /// </summary>
    [JsonPropertyName("first")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ASObject? First { get; set; }
}