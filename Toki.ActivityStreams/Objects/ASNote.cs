using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

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
    /// Who is this note attributed to?
    /// </summary>
    [JsonPropertyName("attributedTo")]
    public ASObject? AttributedTo { get; set; }
    
    /// <summary>
    /// The content of the note.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    /// <summary>
    /// The ASObject this is a quote of. (Misskey extension.)
    /// </summary>
    [JsonPropertyName("_misskey_quote")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ASObject? Quoting { get; set; }
    
    /// <summary>
    /// What this note is replying to.
    /// </summary>
    [JsonPropertyName("inReplyTo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ASObject? InReplyTo { get; set; }
    
    /// <summary>
    /// The raw content of the note. (Misskey extension.)
    /// </summary>
    [JsonPropertyName("_misskey_content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RawContent { get; set; }
    
    /// <summary>
    /// The primary recipients of this note.
    /// </summary>
    [JsonPropertyName("to")]
    [JsonConverter(typeof(ListOrObjectConverter<string>))]
    public IReadOnlyList<string>? To { get; set; }
    
    /// <summary>
    /// The secondary recipients of this note.
    /// </summary>
    [JsonPropertyName("cc")]
    [JsonConverter(typeof(ListOrObjectConverter<string>))]
    public IReadOnlyList<string>? Cc { get; set; }
    
    /// <summary>
    /// Is this post sensitive?
    /// </summary>
    [JsonPropertyName("sensitive")]
    public bool? Sensitive { get; set; }
    
    /// <summary>
    /// The list of tags this note has.
    /// </summary>
    [JsonPropertyName("tag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ASLink>? Tags { get; set; }
    
    /// <summary>
    /// The list of attachments this note has.
    /// </summary>
    [JsonPropertyName("attachment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ASDocument>? Attachments { get; set; }
}