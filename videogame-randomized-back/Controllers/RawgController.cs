using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/rawg")]
public class RawgController(RawgService rawg) : ControllerBase
{
    private const string MissingKeyType = "urn:videogame-randomizer:rawg:missing-api-key";

    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres() =>
        await ProxyGet("/genres");

    [HttpGet("platforms")]
    public async Task<IActionResult> GetPlatforms() =>
        await ProxyGet("/platforms");

    [HttpGet("games")]
    public async Task<IActionResult> GetGames() =>
        await ProxyGet("/games");

    [HttpGet("games/{id:int}")]
    public async Task<IActionResult> GetGameDetails(int id) =>
        await ProxyGet($"/games/{id}");

    [HttpGet("games/{id:int}/screenshots")]
    public async Task<IActionResult> GetGameScreenshots(int id) =>
        await ProxyGet($"/games/{id}/screenshots");

    [HttpGet("games/{id:int}/movies")]
    public async Task<IActionResult> GetGameMovies(int id) =>
        await ProxyGet($"/games/{id}/movies");

    private async Task<IActionResult> ProxyGet(string rawgPath)
    {
        if (!rawg.IsConfigured)
        {
            return Problem(
                title: "RAWG API key is not configured",
                detail: "Set RAWG_API_KEY in the backend environment (.env / docker-compose) to enable RAWG proxy endpoints.",
                statusCode: StatusCodes.Status503ServiceUnavailable,
                type: MissingKeyType);
        }

        var (status, body) = await rawg.GetAsync(rawgPath, Request.Query);

        // RAWG responds with JSON for these endpoints; we preserve status code & body.
        return new ContentResult
        {
            StatusCode = status,
            ContentType = "application/json",
            Content = body
        };
    }
}

