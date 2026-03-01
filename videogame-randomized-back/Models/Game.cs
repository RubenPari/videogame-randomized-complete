using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace videogame_randomized_back.Models;

[FirestoreData]
public class Game
{
    // In Firestore, we usually use the document ID from the database for the unique identifier.
    // However, since this ID comes from an external API (RAWG), we can keep it as a field.
    [FirestoreProperty("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [FirestoreProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("background_image")]
    [JsonPropertyName("background_image")]
    public string? BackgroundImage { get; set; }

    [FirestoreProperty("rating")]
    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [FirestoreProperty("released")]
    [JsonPropertyName("released")]
    public string? Released { get; set; }

    [FirestoreProperty("genres")]
    [JsonPropertyName("genres")]
    public List<Genre>? Genres { get; set; }

    [FirestoreProperty("platforms")]
    [JsonPropertyName("platforms")]
    public List<PlatformWrapper>? Platforms { get; set; }

    [FirestoreProperty("metacritic")]
    [JsonPropertyName("metacritic")]
    public int? Metacritic { get; set; }

    [FirestoreProperty("description_raw")]
    [JsonPropertyName("description_raw")]
    public string? DescriptionRaw { get; set; }

    // Additional fields for the "Vault"
    [FirestoreProperty("personalRating")]
    [JsonPropertyName("personalRating")]
    public int? PersonalRating { get; set; }

    [FirestoreProperty("note")]
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [FirestoreProperty("savedAt")]
    [JsonPropertyName("savedAt")]
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

[FirestoreData]
public class Genre
{
    [FirestoreProperty("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [FirestoreProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("slug")]
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
}

[FirestoreData]
public class PlatformWrapper
{
    [FirestoreProperty("platform")]
    [JsonPropertyName("platform")]
    public Platform Platform { get; set; } = new();
}

[FirestoreData]
public class Platform
{
    [FirestoreProperty("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [FirestoreProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("slug")]
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
}
