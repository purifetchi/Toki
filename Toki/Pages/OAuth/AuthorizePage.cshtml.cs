using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Toki.ActivityPub.Models.OAuth;
using Toki.ActivityPub.OAuth2;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Users;
using Toki.MastodonApi.Helpers;
using Toki.MastodonApi.Schemas.Errors;

namespace Toki.Pages.OAuth;

/// <summary>
/// The model for the "/oauth/authorize" endpoint.
/// </summary>
public class AuthorizePage(
    OAuthRepository repo,
    UserSessionService sessionService,
    OAuthManagementService managementService) : PageModel
{
    /// <summary>
    /// The response type of this request.
    /// </summary>
    [BindProperty(Name = "response_type", SupportsGet = true)]
    public string? ResponseType { get; set; }
    
    /// <summary>
    /// The id of the client app.
    /// </summary>
    [BindProperty(Name = "client_id", SupportsGet = true)]
    public string? ClientId { get; set; }
    
    /// <summary>
    /// The uri of the site we're going to redirect to.
    /// </summary>
    [BindProperty(Name = "redirect_uri", SupportsGet = true)]
    public string? RedirectUri { get; set; }

    /// <summary>
    /// The scopes
    /// </summary>
    [BindProperty(Name = "scope", SupportsGet = true)]
    public string Scope { get; set; } = "read";
    
    /// <summary>
    /// The username of the user.
    /// </summary>
    [BindProperty(Name = "username")]
    public string? Username { get; set; }
    
    /// <summary>
    /// The username of the user.
    /// </summary>
    [BindProperty(Name = "password")]
    public string? Password { get; set; }
    
    /// <summary>
    /// The application.
    /// </summary>
    public OAuthApp? Application { get; private set; }

    /// <summary>
    /// Validates the scopes.
    /// </summary>
    /// <returns>Whether they're valid.</returns>
    private bool ValidateScopes()
    {
        return Application!.ValidateScopes(
            MastodonOAuthHelper.ExpandScopes(Scope.Split(' ')));
    }

    /// <summary>
    /// Validates the redirect uri.
    /// </summary>
    /// <returns>Whether it is valid.</returns>
    private bool ValidateRedirect()
    {
        return RedirectUri is not null &&
               Application!.RedirectUris?.Contains(RedirectUri) == true;
    }
    
    /// <summary>
    /// Invoked on a GET request.
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        if (ResponseType != "code")
            return BadRequest(new MastodonApiError("Validation error: response_code should be 'code'."));
        
        if (ClientId is null)
            return BadRequest(new MastodonApiError("Validation error: client_id is null."));
                
        Application = await repo.FindByClientId(ClientId!);
        if (Application is null)
            return BadRequest(new MastodonApiError("Validation error: Invalid client_id."));
        
        if (!ValidateScopes())
            return BadRequest(new MastodonApiError("Validation error: Requested scopes exceed the application defined scopes."));
        
        if (!ValidateRedirect())
            return BadRequest(new MastodonApiError("Validation error: Application has not pledged this redirect uri."));

        return Page();
    }

    /// <summary>
    /// Invoked on a POST request.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (Username is null || Password is null)
            return BadRequest(new MastodonApiError("Login error: Blank username or password."));

        var user = await sessionService.ValidateCredentials(
            Username,
            Password);

        if (user is null)
            return Unauthorized();

        Application = await repo.FindByClientId(ClientId!);
        if (Application is null)
            return BadRequest(new MastodonApiError("Validation error: Invalid client_id."));

        if (!ValidateScopes())
            return BadRequest(new MastodonApiError("Validation error: Requested scopes exceed the application defined scopes."));

        if (!ValidateRedirect())
            return BadRequest(new MastodonApiError("Validation error: Application has not pledged this redirect uri."));

        var scopes = MastodonOAuthHelper.ExpandScopes(
            Scope.Split(' '));

        var token = await managementService.CreateInactiveToken(
            user,
            Application,
            scopes);

        if (token is null)
            return BadRequest(new MastodonApiError("General error: Unable to create token."));

        return Redirect($"{RedirectUri}?code={token.AuthorizationCode}");
    }
}