using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace videogame_randomized_back.Models;

public class Game
{
    [BsonId]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("background_image")]
    [JsonPropertyName("background_image")]
    public string? BackgroundImage { get; set; }

    [BsonElement("rating")]
    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [BsonElement("released")]
    [JsonPropertyName("released")]
    public string? Released { get; set; }

    [BsonElement("genres")]
    [JsonPropertyName("genres")]
    public List<Genre>? Genres { get; set; }

    [BsonElement("platforms")]
    [JsonPropertyName("platforms")]
    public List<PlatformWrapper>? Platforms { get; set; }

    [BsonElement("metacritic")]
    [JsonPropertyName("metacritic")]
    public int? Metacritic { get; set; }

    [BsonElement("description_raw")]
    [JsonPropertyName("description_raw")]
    public string? DescriptionRaw { get; set; }

    // Additional fields for the "Vault"
    [BsonElement("personalRating")]
    [JsonPropertyName("personalRating")]
    public int? PersonalRating { get; set; }

    [BsonElement("note")]
    [JsonPropertyName("note")]
    public string? Note { get; set; }
    
    [BsonElement("savedAt")]
    [JsonPropertyName("savedAt")]
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

public class Genre
{
    [BsonElement("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [BsonElement("slug")]
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
}

public class PlatformWrapper
{
    [BsonElement("platform")]
    [JsonPropertyName("platform")]
    public Platform Platform { get; set; } = new();
}

public class Platform
{
    [BsonElement("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [BsonElement("slug")]
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
}
