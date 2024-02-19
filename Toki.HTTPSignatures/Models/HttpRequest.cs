using Microsoft.Extensions.Primitives;

namespace Toki.HTTPSignatures.Models;

/// <summary>
/// An HTTP request.
/// </summary>
/// <param name="Method">The HTTP method.</param>
/// <param name="Route">The route of this request.</param>
/// <param name="Headers">The headers of this request.</param>
/// <param name="Body">The body of this request.</param>
public record HttpRequest(
    string Method,
    string Route,
    IReadOnlyDictionary<string, StringValues> Headers,
    string Body);