using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Toki.ActivityPub.Models.OAuth;
using Toki.ActivityPub.Persistence.Repositories;
using Toki.ActivityPub.Users;

namespace Toki.Pages.OAuth;

/// <summary>
/// The model for the "/oauth/authorize" endpoint.
/// </summary>
public class AuthorizePage(
    OAuthRepository repo,
    UserSessionService sessionService) : PageModel
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
    public string? Scope { get; set; } = "read";
    
    /// <summary>
    /// The application.
    /// </summary>
    public OAuthApp? Application { get; private set; }
    
    /// <summary>
    /// Invoked on a GET request.
    /// </summary>
    public async Task OnGetAsync()
    {
        Application = await repo.FindByClientId(ClientId!);
    }

    /// <summary>
    /// Invoked on a POST request.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(
        [FromForm] string username,
        [FromForm] string password)
    {
        var user = await sessionService.ValidateCredentials(
            username,
            password);

        if (user is null)
            return Unauthorized();
        
        // TODO: Create an OAuth2 token here.
        
        return StatusCode(
            202, 
            $"signed in as {user.Handle}, make token ples");
    }
}