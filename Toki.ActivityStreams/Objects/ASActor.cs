using System.Text.Json.Serialization;
using Toki.ActivityStreams.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams actor.
/// </summary>
public class ASActor : ASObject
{
    /// <summary>
    /// Constructs a new ASActor.
    /// </summary>
    public ASActor(string type = "Person")
        : base(type)
    {
        
    }
    
    /// <summary>
    /// The name of the actor.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// The display name.
    /// </summary>
    [JsonPropertyName("preferredUsername")]
    public string? PreferredUsername { get; set; }
    
    /// <summary>
    /// The bio of this actor.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Bio { get; set; }
    
    /// <summary>
    /// The inbox.
    /// </summary>
    [JsonPropertyName("inbox")]
    public ASObject? Inbox { get; set; }
    
    /// <summary>
    /// The outbox.
    /// </summary>
    [JsonPropertyName("outbox")]
    public ASObject? Outbox { get; set; }
    
    /// <summary>
    /// The icon.
    /// </summary>
    [JsonPropertyName("icon")]
    [JsonConverter(typeof(ForceSingleObjectConverter<ASImage>))]
    public ASImage? Icon { get; set; }
    
    /// <summary>
    /// The banner.
    /// </summary>
    [JsonPropertyName("image")]
    [JsonConverter(typeof(ForceSingleObjectConverter<ASImage>))]
    public ASImage? Banner { get; set; }
    
    /// <summary>
    /// The public key.
    /// </summary>
    [JsonPropertyName("publicKey")]
    public ASPublicKey? PublicKey { get; set; }
    
    /// <summary>
    /// The followers collection.
    /// </summary>
    [JsonPropertyName("followers")]
    public ASObject? Followers { get; set; }
    
    /// <summary>
    /// The following collection.
    /// </summary>
    [JsonPropertyName("following")]
    public ASObject? Following { get; set; }
    
    /// <summary>
    /// The pinned (featured) notes collection.
    /// </summary>
    [JsonPropertyName("featured")]
    public ASObject? Featured { get; set; }
    
    /// <summary>
    /// The url of the actor.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    /// <summary>
    /// Does this actor manually approve followers?
    /// </summary>
    [JsonPropertyName("manuallyApprovesFollowers")]
    public bool ManuallyApprovesFollowers { get; set; }
    
    /// <summary>
    /// Should this actor be hidden from discovery?
    /// </summary>
    [JsonPropertyName("invisible")]
    public bool Invisible { get; set; }
    
    /// <summary>
    /// The endpoints.
    /// </summary>
    [JsonPropertyName("endpoints")]
    public ASEndpoints? Endpoints { get; set; }
    
    /// <summary>
    /// The list of attachments this user has.
    /// </summary>
    [JsonPropertyName("attachment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ASLink>? Attachments { get; set; }
}