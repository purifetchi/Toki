namespace Toki.ActivityPub.Models;

/// <summary>
/// A keypair.
/// </summary>
public class Keypair : RemoteableModel
{
    /// <summary>
    /// The owner of the keypair.
    /// </summary>
    public User? Owner { get; set; }
    
    /// <summary>
    /// The public key.
    /// </summary>
    public required string PublicKey { get; init; }
    
    /// <summary>
    /// The private key. (Null for remote users)
    /// </summary>
    public string? PrivateKey { get; set; }
}