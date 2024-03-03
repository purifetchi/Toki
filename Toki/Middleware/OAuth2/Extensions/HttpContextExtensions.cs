using Toki.ActivityPub.Models.OAuth;

namespace Toki.Middleware.OAuth2.Extensions;

/// <summary>
/// Extensions for the <see cref="HttpContext"/> class.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// The name of the token in the items dict.
    /// </summary>
    private const string ITEM_NAME = "OAuthSession";
    
    /// <summary>
    /// Gets the <see cref="OAuthToken"/> from the context.
    /// </summary>
    /// <param name="ctx">The http context.</param>
    /// <returns>The token.</returns>
    public static OAuthToken? GetOAuthToken(this HttpContext ctx) =>
        ctx.Items.TryGetValue(ITEM_NAME, out var token) ? token as OAuthToken : null;

    /// <summary>
    /// Sets the <see cref="OAuthToken"/> to the context.
    /// </summary>
    /// <param name="ctx">The http context.</param>
    /// <param name="token">The token;</param>
    public static void SetOAuthToken(this HttpContext ctx, OAuthToken token) =>
        ctx.Items[ITEM_NAME] = token;
}