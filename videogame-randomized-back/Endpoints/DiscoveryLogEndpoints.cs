using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using videogame_randomized_back.Data;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Endpoints;

public static class DiscoveryLogEndpoints
{
    public static void MapDiscoveryLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/discovery-log")
            .WithTags("Discovery Log")
            .RequireAuthorization();

        group.MapGet("/", GetDiscoveryLog);
        group.MapPost("/", SaveDiscoveryLog);
        group.MapDelete("/", ClearDiscoveryLog);
    }

    private static string GetUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Get all previously discovered game IDs for the current user
    /// </summary>
    private static async Task<IResult> GetDiscoveryLog(
        ClaimsPrincipal user,
        AppDbContext db)
    {
        var userId = GetUserId(user);
        var entries = await db.DiscoveryLog
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.DiscoveredAt)
            .Select(e => new { id = e.GameExternalId, name = e.GameName })
            .ToListAsync();

        return Results.Ok(entries);
    }

    /// <summary>
    /// Save the current session's discovered games to the user's log
    /// </summary>
    private static async Task<IResult> SaveDiscoveryLog(
        [FromBody] List<DiscoveryLogDto>? entries,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        if (entries == null || entries.Count == 0)
            return Results.Ok(new { saved = 0 });

        var userId = GetUserId(user);

        // Get existing game IDs for this user to avoid duplicates
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

        if (newEntries.Count <= 0) return Results.Ok(new { saved = newEntries.Count });
        
        db.DiscoveryLog.AddRange(newEntries);
        await db.SaveChangesAsync();

        return Results.Ok(new { saved = newEntries.Count });
    }

    /// <summary>
    /// Clear all discovery log entries for the current user
    /// </summary>
    private static async Task<IResult> ClearDiscoveryLog(
        ClaimsPrincipal user,
        AppDbContext db)
    {
        var userId = GetUserId(user);
        await db.DiscoveryLog
            .Where(e => e.UserId == userId)
            .ExecuteDeleteAsync();

        return Results.Ok(new { message = "Discovery log cleared" });
    }
}

public abstract record DiscoveryLogDto(int Id, string Name);
