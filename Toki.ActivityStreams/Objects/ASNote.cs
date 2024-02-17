using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams note.
/// </summary>
public class ASNote : ASObject
{
    /// <summary>
    /// Constructs a new ASNote.
    /// </summary>
    public ASNote()
        : base("Note")
    {
        
    }
    
    /// <summary>
    /// The content of the note.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}