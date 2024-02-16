using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toki.ActivityPub.WebFinger;

/// <summary>
/// The WebFinger resolver.
/// </summary>
public class WebFingerResolver(IHttpClientFactory httpClientFactory)
{
    /// <summary>
    /// Looks up a WebFinger user.
    /// </summary>
    /// <param name="instance">The instance to look up on.</param>
    /// <param name="handle">The handle of the user to look up.</param>
    /// <returns>The WebFinger response.</returns>
    public async Task<WebFingerResponse?> Finger(
        string instance, 
        string handle)
    {
        const string webFingerEndpoint = "/.well-known/webfinger?resource=";

        var url = instance + webFingerEndpoint;
        var acct = $"acct:{handle.TrimStart('@')}";

        var client = httpClientFactory.CreateClient();
        var resp = await client.GetFromJsonAsync<WebFingerResponse>(url + acct);

        return resp;
    }
}