using System.Security.Cryptography;
using System.Text;
using Toki.HTTPSignatures.Models;

namespace Toki.HTTPSignatures;

/// <summary>
/// The HTTP signature validator.
/// </summary>
public class HttpSignatureValidator
{
    /// <summary>
    /// Validates a request.
    /// </summary>
    /// <returns></returns>
    public bool Validate(
        Signature signature,
        string publicPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicPem);
        
        return rsa.VerifyHash(
            Encoding.UTF8.GetBytes(signature.MessageToSign), 
            Convert.FromBase64String(signature.SignedDigest), 
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }
}