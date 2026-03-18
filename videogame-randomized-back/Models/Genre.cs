using System.Text.Json.Serialization;

namespace videogame_randomized_back.Models;

public class Genre
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
    
    public List<Game> Games { get; set; } = [];
}
