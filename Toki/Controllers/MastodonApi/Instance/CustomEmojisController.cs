using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Toki.ActivityPub.Emojis;
using Toki.MastodonApi.Renderers;
using Toki.MastodonApi.Schemas.Objects;

namespace Toki.Controllers.MastodonApi.Instance;

/// <summary>
/// The controller for the "/api/v1/custom_emojis" endpoint.
/// </summary>
[ApiController]
[EnableCors("MastodonAPI")]
[Route("/api/v1/custom_emojis")]
public class CustomEmojisController(
    EmojiService emojiService,
    StatusRenderer statusRenderer) : ControllerBase
{
    /// <summary>
    /// Returns custom emojis that are available on the server.
    /// </summary>
    /// <returns>Array of <see cref="CustomEmoji"/></returns>
    [HttpGet]
    [Produces("application/json")]
    public async Task<IEnumerable<CustomEmoji>> GetCustomEmojis()
    {
        var emoji = await emojiService.GetAllLocalEmoji();

        return emoji
            .Select(statusRenderer.RenderEmojiFrom);
    }
}