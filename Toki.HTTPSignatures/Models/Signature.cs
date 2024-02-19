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
            .Select(part => part.Split('='))
            .ToDictionary(parts => parts[0], parts => parts[1].Trim('"'));

        var keyId = sigParts["keyId"];
        var signedDigest = sigParts["signature"];
        
        var sb = new StringBuilder();
        foreach (var header in sigParts["headers"].Split(' '))
        {
            const string requestTarget = "(request-target)";

            if (header == requestTarget)
            {
                sb.AppendLine(
                    $"{requestTarget}: {request.Method.ToLowerInvariant()} {request.Route.ToLowerInvariant()}");
            }
            else
            {
                sb.AppendLine(
                    $"{header}: {request.Headers[CultureInfo.InvariantCulture.TextInfo.ToTitleCase(header)]}");
            }
        }
        
        return new Signature(keyId, sb.ToString(), signedDigest);
    }
}
