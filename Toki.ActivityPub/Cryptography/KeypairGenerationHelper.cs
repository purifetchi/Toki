using System.Security.Cryptography;
using Toki.ActivityPub.Models;

namespace Toki.ActivityPub.Cryptography;

/// <summary>
/// The keypair generator.
/// </summary>
public static class KeypairGenerationHelper
{
    /// <summary>
    /// Generates an RSA keypair.
    /// </summary>
    /// <returns>The RSA keypair.</returns>
    public static Keypair GenerateKeypair()
    {
        using var rsa = RSA.Create();
        
        return new Keypair
        {
            Id = Ulid.NewUlid(),
            PublicKey = rsa.ExportRSAPublicKeyPem(),
            PrivateKey = rsa.ExportRSAPrivateKeyPem()
        };
    }
}