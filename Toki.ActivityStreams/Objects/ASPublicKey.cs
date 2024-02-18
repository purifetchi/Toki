using System.Text.Json.Serialization;

namespace Toki.ActivityStreams.Objects;

/// <summary>
/// An ActivityStreams public key.
/// </summary>
public class ASPublicKey : ASObject
{
    /// <summary>
    /// Constructs a new public key.
    /// </summary>
    public ASPublicKey()
        : base(string.Empty)
    {
        
    }
    
    /// <summary>
    /// The owner of the public key.
    /// </summary>
    [JsonPropertyName("owner")]
    public ASObject? Owner { get; set; }
    
    /// <summary>
    /// The PEM encoded public key.
    /// </summary>
    [JsonPropertyName("publicKeyPem")]
    public string? PublicKeyPem { get; set; }
}