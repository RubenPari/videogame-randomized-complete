using videogame_randomized_back.Models;

namespace videogame_randomized_back.DTOs;

public record GameDto(
    int Id,
    string Name,
    string? BackgroundImage,
    double Rating,
    string? Released,
    List<GenreDto>? Genres,
    List<PlatformDto>? Platforms,
    int? Metacritic,
    string? DescriptionRaw,
    int? PersonalRating,
    string? Note,
    DateTime SavedAt
);

public record CreateGameDto(
    int Id,
    string Name,
    string? BackgroundImage,
    double Rating,
    string? Released,
    List<GenreDto>? Genres,
    List<PlatformDto>? Platforms,
    int? Metacritic,
    string? DescriptionRaw
);

public record UpdateGameDto(
    int? PersonalRating,
    string? Note
);

public record GenreDto(
    int Id,
    string Name,
    string? Slug
);

public record PlatformDto(
    int Id,
    string Name,
    string? Slug
);

public record GameStatsDto(
    int TotalGames,
    double AverageRating,
    Dictionary<string, int> GenreCount,
    Dictionary<string, int> PlatformCount
);
