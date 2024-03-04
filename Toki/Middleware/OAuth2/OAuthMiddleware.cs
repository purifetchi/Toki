using Toki.ActivityPub.Models.OAuth;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.MastodonApi.Schemas.Errors;
using Toki.Middleware.OAuth2.Extensions;

namespace Toki.Middleware.OAuth2;

/// <summary>
/// The OAuth middleware.
/// </summary>
public class OAuthMiddleware(
    OAuthRepository repo,
    ILogger<OAuthMiddleware> logger) : IMiddleware
{
    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var oauthAttrib = context.GetEndpoint()?
            .Metadata
            .GetMetadata<OAuthAttribute>();

        if (oauthAttrib is null)
        {
            await next(context);
            return;
        }

        var token = await GetOAuthToken(context);
        if (oauthAttrib.ManualScopeValidation)
        {
            await next(context);
            return;
        }
        
        if (token is null)
        {
            await ThrowError(
                context,
                StatusCodes.Status401Unauthorized,
                new MastodonApiError("No OAuth code present."));
            
            return;
        }

        if (token.Scopes?.Contains(oauthAttrib.Scope) != true)
        {
            await ThrowError(
                context,
                StatusCodes.Status401Unauthorized,
                new MastodonApiError("Scope is outside of the pledged scopes."));
            
            return;
        }

        logger.LogInformation($"Accessing OAuth2 protected resource: {oauthAttrib.Scope}");
        await next(context);
    }

    /// <summary>
    /// Throws an error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="response">The response to write.</param>
    /// <param name="ctx">The http context.</param>
    private async Task ThrowError(HttpContext ctx, int code, MastodonApiError response)
    {
        ctx.Response.StatusCode = code;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(response);
    }

    /// <summary>
    /// Gets the OAuth token from the context.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <returns>The token, if one exists.</returns>
    private async Task<OAuthToken?> GetOAuthToken(HttpContext ctx)
    {
        var cachedToken = ctx.GetOAuthToken();
        if (cachedToken is not null)
            return cachedToken;

        var token = await FetchTokenFromHeader(ctx);
        if (token is null)
            return null;

        ctx.SetOAuthToken(token);
        return token;
    }

    /// <summary>
    /// Fetches the token from the header.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <returns>The token, if it exists.</returns>
    private async Task<OAuthToken?> FetchTokenFromHeader(HttpContext ctx)
    {
        var header = ctx.Request
            .Headers
            .Authorization
            .ToString();

        if (string.IsNullOrEmpty(header))
            return null;

        var code = header.Split(' ')
            .Last();

        var token = await repo.FindTokenByCode(code);
        return token;
    }
}