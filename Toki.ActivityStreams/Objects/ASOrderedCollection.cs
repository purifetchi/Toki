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
    /// An ordered collection of items.
    /// </summary>
    [JsonPropertyName("orderedItems")]
    public IList<TObject> OrderedItems { get; set; } = null!;
}