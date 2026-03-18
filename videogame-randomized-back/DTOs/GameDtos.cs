namespace videogame_randomized_back.DTOs;

public record GameDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? BackgroundImage { get; init; }
    public double Rating { get; init; }
    public string? Released { get; init; }
    public List<GenreDto>? Genres { get; init; }
    public List<PlatformDto>? Platforms { get; init; }
    public int? Metacritic { get; init; }
    public string? DescriptionRaw { get; init; }
    public int? PersonalRating { get; init; }
    public string? Note { get; init; }
    public DateTime SavedAt { get; init; }
    public string UserId { get; init; } = string.Empty;
}

public record CreateGameDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? BackgroundImage { get; init; }
    public double Rating { get; init; }
    public string? Released { get; init; }
    public List<GenreDto>? Genres { get; init; }
    public List<PlatformDto>? Platforms { get; init; }
    public int? Metacritic { get; init; }
    public string? DescriptionRaw { get; init; }
}

public record UpdateGameDto
{
    public int? Id { get; init; }
    public string? Name { get; init; }
    public string? BackgroundImage { get; init; }
    public double? Rating { get; init; }
    public string? Released { get; init; }
    public List<GenreDto>? Genres { get; init; }
    public List<PlatformDto>? Platforms { get; init; }
    public int? Metacritic { get; init; }
    public string? DescriptionRaw { get; init; }
    public int? PersonalRating { get; init; }
    public string? Note { get; init; }
}

public record GenreDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
}

public record PlatformDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
}

public record GameStatsDto
{
    public int TotalGames { get; init; }
    public double AverageRating { get; init; }
    public Dictionary<string, int> GenreCount { get; init; } = new();
    public Dictionary<string, int> PlatformCount { get; init; } = new();
}
