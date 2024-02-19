using System.Globalization;
using System.Text;

namespace Toki.HTTPSignatures.Models;

/// <summary>
/// A signature.
/// </summary>
/// <param name="KeyId">The ID of the key.</param>
/// <param name="MessageToSign">The message to be signed.</param>
/// <param name="SignedDigest">The signed digest.</param>
public record Signature(
    string KeyId,
    string MessageToSign,
    string SignedDigest)
{
    /// <summary>
    /// Constructs a new signature from an HTTP request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The signature.</returns>
    public static Signature? FromHttpRequest(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Signature", out var sigHeader))
            return null;

        var sigParts = sigHeader[0]!.Split(',')
            .Select(part => part.Split('=', 2))
            .ToDictionary(parts => parts[0], parts => parts[1].Trim('"'));

        var keyId = sigParts["keyId"];
        var signedDigest = sigParts["signature"];
        
        var message = sigParts["headers"].Split(' ')
            .Aggregate(string.Empty, (acc, header) => header switch
            {
                "(request-target)" => acc + $"(request-target): {request.Method.ToLowerInvariant()} {request.Route.ToLowerInvariant()}\n",
                _ => acc + $"{header}: {request.Headers[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(header)]}\n"
            }).TrimEnd();
        
        return new Signature(keyId, message, signedDigest);
    }
}
