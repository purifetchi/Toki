namespace Toki.Extensions;

public static class HttpRequestExtensions
{
    public static HTTPSignatures.Models.HttpRequest ToTokiHttpRequest(this HttpRequest request)
    {
        return new HTTPSignatures.Models.HttpRequest(
            request.Method,
            request.Path,
            request.Headers.ToDictionary(
                key => key.Key.ToLower(), 
                value => value.Value),
            "");
    }
}