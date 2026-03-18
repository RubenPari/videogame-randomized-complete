namespace videogame_randomized_back.DTOs;

public record GameDto;

public abstract record CreateGameDto(
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

public abstract record UpdateGameDto(
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

public record GameStatsDto;
