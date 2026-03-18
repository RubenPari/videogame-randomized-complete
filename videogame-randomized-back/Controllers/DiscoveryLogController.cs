using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using videogame_randomized_back.Data;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/discovery-log")]
[Authorize]
public class DiscoveryLogController(AppDbContext db) : ControllerBase
{
    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Gets all previously discovered game IDs for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DiscoveryLogResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DiscoveryLogResponseDto>>> GetDiscoveryLog()
    {
        var userId = GetUserId();
        var entries = await db.DiscoveryLog
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.DiscoveredAt)
            .Select(e => new DiscoveryLogResponseDto(e.GameExternalId, e.GameName))
            .ToListAsync();

        return Ok(entries);
    }

    /// <summary>
    /// Saves the current session's discovered games to the user's log
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaveDiscoveryLogResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SaveDiscoveryLogResultDto>> SaveDiscoveryLog([FromBody] List<DiscoveryLogDto>? entries)
    {
        if (entries is null || entries.Count == 0)
            return Ok(new SaveDiscoveryLogResultDto(0));

        var userId = GetUserId();

        var incomingIds = entries.Select(e => e.Id).ToList();

        var existingIds = await db.DiscoveryLog
            .Where(e => e.UserId == userId && incomingIds.Contains(e.GameExternalId))
            .Select(e => e.GameExternalId)
            .ToListAsync();

        var newEntries = entries
            .Where(e => !existingIds.Contains(e.Id))
            .Select(e => new DiscoveryLogEntry
            {
                UserId = userId,
                GameExternalId = e.Id,
                GameName = e.Name,
                DiscoveredAt = DateTime.UtcNow
            })
            .ToList();

        if (newEntries.Count <= 0) return Ok(new SaveDiscoveryLogResultDto(newEntries.Count));
        
        db.DiscoveryLog.AddRange(newEntries);
        await db.SaveChangesAsync();

        return Ok(new SaveDiscoveryLogResultDto(newEntries.Count));
    }

    /// <summary>
    /// Clears all discovery log entries for the current user
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> ClearDiscoveryLog()
    {
        var userId = GetUserId();
        
        await db.DiscoveryLog
            .Where(e => e.UserId == userId)
            .ExecuteDeleteAsync();

        return Ok(new { message = "Discovery log cleared" });
    }
}