using Microsoft.EntityFrameworkCore;
using videogame_randomized_back.Data;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

public class DiscoveryLogService(AppDbContext db) : IDiscoveryLogService
{
    public async Task<List<DiscoveryLogResponseDto>> GetEntriesForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await db.DiscoveryLog
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.DiscoveredAt)
            .Select(e => new DiscoveryLogResponseDto(e.GameExternalId, e.GameName))
            .ToListAsync(cancellationToken);
    }

    public async Task<SaveDiscoveryLogResultDto> SaveEntriesAsync(
        string userId,
        List<DiscoveryLogDto>? entries,
        CancellationToken cancellationToken = default)
    {
        if (entries is null || entries.Count == 0)
            return new SaveDiscoveryLogResultDto(0);

        var incomingIds = entries.Select(e => e.Id).ToList();

        var existingIds = await db.DiscoveryLog
            .Where(e => e.UserId == userId && incomingIds.Contains(e.GameExternalId))
            .Select(e => e.GameExternalId)
            .ToListAsync(cancellationToken);

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

        if (newEntries.Count <= 0)
            return new SaveDiscoveryLogResultDto(0);

        db.DiscoveryLog.AddRange(newEntries);
        await db.SaveChangesAsync(cancellationToken);

        return new SaveDiscoveryLogResultDto(newEntries.Count);
    }

    public async Task ClearForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        await db.DiscoveryLog
            .Where(e => e.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
