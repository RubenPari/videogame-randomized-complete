using System.Text.Json.Serialization;

namespace videogame_randomized_back.Models;

public class Game
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("background_image")]
    public string? BackgroundImage { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [JsonPropertyName("released")]
    public string? Released { get; set; }

    [JsonPropertyName("genres")]
    public List<Genre> Genres { get; set; } = new();

    [JsonPropertyName("platforms")]
    public List<Platform> Platforms { get; set; } = new();

    [JsonPropertyName("metacritic")]
    public int? Metacritic { get; set; }

    [JsonPropertyName("description_raw")]
    public string? DescriptionRaw { get; set; }

    [JsonPropertyName("personalRating")]
    public int? PersonalRating { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("savedAt")]
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
