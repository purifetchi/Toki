using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Toki.ActivityPub.Configuration;
using Toki.Services.Usage;
using Toki.Services.Usage.Models;

namespace Toki.Pages.Landing;

/// <summary>
/// The landing index page.
/// </summary>
public class Index(
    IOptions<InstanceConfiguration> opts,
    UsageService service) : PageModel
{
    /// <summary>
    /// The configuration for this Toki instance.
    /// </summary>
    public InstanceConfiguration Configuration { get; init; } = opts.Value;
    
    /// <summary>
    /// The usage statistics of this instance.
    /// </summary>
    public UsageStatistics? Statistics { get; private set; }
    
    public async Task OnGetAsync()
    {
        Statistics = await service.GetStatistics();
    }
}