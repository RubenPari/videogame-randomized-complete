using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/youtube")]
public class YoutubeController(YoutubeService youtube) : ControllerBase
{
    private const string MissingKeyType = "urn:videogame-randomizer:youtube:missing-api-key";

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (!youtube.IsConfigured)
        {
            return Problem(
                title: "YouTube API key is not configured",
                detail: "Set YOUTUBE_API_KEY in the backend environment (.env / docker-compose) to enable YouTube search.",
                statusCode: StatusCodes.Status503ServiceUnavailable,
                type: MissingKeyType);
        }

        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "q is required" });
        }

        var (status, body) = await youtube.SearchAsync(q);

        // On success, reduce payload to { videoId } to keep frontend simple/stable.
        if (status >= 200 && status < 300)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var items = doc.RootElement.TryGetProperty("items", out var itemsEl) ? itemsEl : default;
                if (items.ValueKind == JsonValueKind.Array && items.GetArrayLength() > 0)
                {
                    var first = items[0];
                    if (first.TryGetProperty("id", out var idEl) &&
                        idEl.TryGetProperty("videoId", out var videoIdEl) &&
                        videoIdEl.ValueKind == JsonValueKind.String)
                    {
                        return Ok(new { videoId = videoIdEl.GetString() });
                    }
                }

                return Ok(new { videoId = (string?)null });
            }
            catch
            {
                // Fall through to returning raw body for easier debugging.
            }
        }

        return new ContentResult
        {
            StatusCode = status,
            ContentType = "application/json",
            Content = body
        };
    }
}

