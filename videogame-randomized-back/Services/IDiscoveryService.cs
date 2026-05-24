using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Services;

public interface IDiscoveryService
{
    Task<DiscoveryRandomResponseDto> GetRandomGameAsync(
        string userId,
        DiscoveryRandomRequestDto request,
        CancellationToken cancellationToken);
}
