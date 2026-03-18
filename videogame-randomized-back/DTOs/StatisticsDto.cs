namespace videogame_randomized_back.DTOs;

public class StatisticsDto
{
    public int TotalGames { get; set; }
    public double AverageRating { get; set; }
    public Dictionary<string, int> GenreCount { get; set; } = new();
    public Dictionary<string, int> PlatformCount { get; set; } = new();
}
