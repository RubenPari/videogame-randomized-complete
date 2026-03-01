using Microsoft.Extensions.Configuration;
using Google.Cloud.Firestore;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

public class SavedGamesService
{
    private readonly FirestoreDb _db;
    private const string CollectionName = "Games";

    public SavedGamesService(IConfiguration configuration)
    {
        var projectId = configuration["GoogleCloud:ProjectId"];
        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentNullException(nameof(projectId), "GoogleCloud:ProjectId not configured");
        }
        _db = FirestoreDb.Create(projectId);
    }

    public async Task<List<Game>> GetAsync()
    {
        Query query = _db.Collection(CollectionName);
        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
        return querySnapshot.Documents.Select(d => d.ConvertTo<Game>()).ToList();
    }

    public async Task<Game?> GetAsync(int id)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id.ToString());
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<Game>();
        }
        return null;
    }

    public async Task CreateAsync(Game newGame)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(newGame.Id.ToString());
        await docRef.SetAsync(newGame);
    }

    public async Task UpdateAsync(int id, Game updatedGame)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id.ToString());
        await docRef.SetAsync(updatedGame, SetOptions.MergeAll);
    }

    public async Task RemoveAsync(int id)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id.ToString());
        await docRef.DeleteAsync();
    }

    public async Task RemoveAllAsync()
    {
        Query query = _db.Collection(CollectionName);
        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

        // In production, deleting all documents in a collection should ideally be done in batches or through a Cloud Function.
        foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
        {
            await documentSnapshot.Reference.DeleteAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id.ToString());
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists;
    }

    public async Task<List<Game>> SearchAsync(string query)
    {
        // Firestore non supporta query `LIKE` %query% o Regex in modo nativo per la ricerca full-text on string fields (in modo semplice e case-insensitive come Mongo).
        // Un workaround temporaneo è fetchare tutto e filtrare in memoria. Poiché stiamo migrando per questo caso d'uso.
        var allGames = await GetAsync();
        if (string.IsNullOrWhiteSpace(query)) return allGames;

        return allGames.Where(g => g.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<StatisticsDto> GetStatisticsAsync()
    {
        var games = await GetAsync();

        if (games.Count == 0)
        {
            return new StatisticsDto
            {
                TotalGames = 0,
                AverageRating = 0,
                GenreCount = new Dictionary<string, int>(),
                PlatformCount = new Dictionary<string, int>()
            };
        }

        var stats = new StatisticsDto
        {
            TotalGames = games.Count,
            AverageRating = games.Average(g => g.Rating),
            GenreCount = new Dictionary<string, int>(),
            PlatformCount = new Dictionary<string, int>()
        };

        foreach (var game in games)
        {
            if (game.Genres != null)
            {
                foreach (var genre in game.Genres)
                {
                    if (!stats.GenreCount.ContainsKey(genre.Name))
                        stats.GenreCount[genre.Name] = 0;
                    stats.GenreCount[genre.Name]++;
                }
            }

            if (game.Platforms != null)
            {
                foreach (var pWrapper in game.Platforms)
                {
                    var pName = pWrapper.Platform.Name;
                    if (!stats.PlatformCount.ContainsKey(pName))
                        stats.PlatformCount[pName] = 0;
                    stats.PlatformCount[pName]++;
                }
            }
        }

        return stats;
    }
}

public class StatisticsDto
{
    public int TotalGames { get; set; }
    public double AverageRating { get; set; }
    public Dictionary<string, int> GenreCount { get; set; } = new();
    public Dictionary<string, int> PlatformCount { get; set; } = new();
}
