using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/discovery-log")]
[Authorize]
public class DiscoveryLogController(IDiscoveryLogService discoveryLog) : ControllerBase
{
    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Gets all previously discovered game IDs for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DiscoveryLogResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DiscoveryLogResponseDto>>> GetDiscoveryLog(
        CancellationToken cancellationToken)
    {
        var entries = await discoveryLog.GetEntriesForUserAsync(GetUserId(), cancellationToken);
        return Ok(entries);
    }

    /// <summary>
    /// Saves the current session's discovered games to the user's log
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaveDiscoveryLogResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SaveDiscoveryLogResultDto>> SaveDiscoveryLog(
        [FromBody] List<DiscoveryLogDto>? entries,
        CancellationToken cancellationToken)
    {
        var result = await discoveryLog.SaveEntriesAsync(GetUserId(), entries, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Clears all discovery log entries for the current user
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> ClearDiscoveryLog(CancellationToken cancellationToken)
    {
        await discoveryLog.ClearForUserAsync(GetUserId(), cancellationToken);
        return Ok(new { message = "Discovery log cleared" });
    }
}
