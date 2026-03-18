using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using videogame_randomized_back.Data;
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
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<object>>> GetDiscoveryLog()
    {
        var userId = GetUserId();
        var entries = await db.DiscoveryLog
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.DiscoveredAt)
            .Select(e => new { id = e.GameExternalId, name = e.GameName })
            .ToListAsync();

        return Ok(entries);
    }

    /// <summary>
    /// Saves the current session's discovered games to the user's log
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> SaveDiscoveryLog([FromBody] List<DiscoveryLogDto>? entries)
    {
        if (entries == null || entries.Count == 0)
            return Ok(new { saved = 0 });

        var userId = GetUserId();

        var existingIds = await db.DiscoveryLog
            .Where(e => e.UserId == userId)
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

        if (newEntries.Count <= 0) return Ok(new { saved = newEntries.Count });

        db.DiscoveryLog.AddRange(newEntries);
        await db.SaveChangesAsync();

        return Ok(new { saved = newEntries.Count });
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

public abstract record DiscoveryLogDto(int Id, string Name);
