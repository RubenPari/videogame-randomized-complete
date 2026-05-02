using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/discovery")]
[Authorize]
public class DiscoveryController(
    DiscoveryService discovery,
    RawgService rawg) : ControllerBase
{
    private const string MissingKeyType = "urn:videogame-randomizer:rawg:missing-api-key";

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("random")]
    public async Task<IActionResult> GetRandom(
        [FromQuery] DiscoveryRandomRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!rawg.IsConfigured)
        {
            return Problem(
                title: "RAWG API key is not configured",
                detail: "Set RAWG_API_KEY in the backend environment (.env / docker-compose) to enable discovery.",
                statusCode: StatusCodes.Status503ServiceUnavailable,
                type: MissingKeyType);
        }

        var dto = await discovery.GetRandomGameAsync(GetUserId(), request, cancellationToken);

        if (dto.IsUpstreamFailure)
        {
            return Problem(
                title: "RAWG temporarily unavailable",
                detail: dto.Error ?? "Unable to reach the game catalog.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        return Ok(dto);
    }
}
