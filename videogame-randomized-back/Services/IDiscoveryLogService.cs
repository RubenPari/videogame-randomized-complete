using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Services;

public interface IDiscoveryLogService
{
    Task<List<DiscoveryLogResponseDto>> GetEntriesForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<SaveDiscoveryLogResultDto> SaveEntriesAsync(string userId, List<DiscoveryLogDto>? entries, CancellationToken cancellationToken = default);

    Task ClearForUserAsync(string userId, CancellationToken cancellationToken = default);
}
