using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.MastodonApi.Schemas.Requests.Search;
using Toki.MastodonApi.Schemas.Responses.Search;
using Toki.Middleware.OAuth2;
using Toki.Middleware.OAuth2.Extensions;
using Toki.Services.Search;

namespace Toki.Controllers.MastodonApi.Search;

/// <summary>
/// Controller for the "/api/v2/search" endpoint.
/// </summary>
[ApiController]
[EnableCors("MastodonAPI")]
[Route("/api/v2/search")]
public class SearchController(
    SearchService searchService) : ControllerBase
{
    /// <summary>
    /// Performs a search.
    /// </summary>
    /// <returns>The resulting <see cref="SearchResponse"/></returns>
    [HttpGet]
    [OAuth(manualScopeValidation: true)]
    [Route("")]
    public async Task<ActionResult<SearchResponse>> Search(
        [FromQuery] SearchRequest request)
    {
        return await searchService.Search(
            request,
            HttpContext.GetOAuthToken()?.User);
    }
}