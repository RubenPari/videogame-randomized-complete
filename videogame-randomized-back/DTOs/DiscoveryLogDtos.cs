namespace videogame_randomized_back.DTOs;

public record DiscoveryLogDto(int Id, string Name);

public record DiscoveryLogResponseDto(int Id, string Name);

public record SaveDiscoveryLogResultDto(int Saved);
