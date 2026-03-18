namespace videogame_randomized_back.DTOs;

public class StatisticsDto
{
    public int TotalGames { get; init; }
    public double AverageRating { get; init; }
    public Dictionary<string, int> GenreCount { get; init; } = new();
    public Dictionary<string, int> PlatformCount { get; init; } = new();
}
