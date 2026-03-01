using Microsoft.Extensions.Options;
using MongoDB.Bson; // Added missing using
using MongoDB.Driver;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

public class SavedGamesService
{
    private readonly IMongoCollection<Game> _gamesCollection;

    public SavedGamesService(IConfiguration config)
    {
        // Connection string should be like: mongodb://host:port/DatabaseName
        // But for flexibility, let's parse the connection string or just use the database name from config if provided separately
        // Here I'll assume the connection string includes the database name or we default to "VideogameDB"
        
        var connectionString = config.GetConnectionString("DefaultConnection");
        var mongoUrl = MongoUrl.Create(connectionString);
        var mongoClient = new MongoClient(mongoUrl);
        var databaseName = mongoUrl.DatabaseName ?? "VideogameDB";
        var mongoDatabase = mongoClient.GetDatabase(databaseName);

        _gamesCollection = mongoDatabase.GetCollection<Game>("Games");
    }

    public async Task<List<Game>> GetAsync() =>
        await _gamesCollection.Find(_ => true).ToListAsync();

    public async Task<Game?> GetAsync(int id) =>
        await _gamesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Game newGame)
    {
        // Use ReplaceOne with IsUpsert = true to handle potential duplicates or re-saves gracefully, 
        // although the requirement says "Create". 
        // If "Create" strictly means "fail if exists", use InsertOne.
        // Frontend checks if saved first, but race conditions exist. 
        // I'll stick to InsertOne as per standard POST behavior, letting it throw if ID exists.
        // Actually, to be safe and robust, let's check first or catch the exception in controller.
        // Let's use InsertOneAsync.
        await _gamesCollection.InsertOneAsync(newGame);
    }

    public async Task UpdateAsync(int id, Game updatedGame) =>
        await _gamesCollection.ReplaceOneAsync(x => x.Id == id, updatedGame);

    public async Task RemoveAsync(int id) =>
        await _gamesCollection.DeleteOneAsync(x => x.Id == id);

    public async Task RemoveAllAsync() =>
        await _gamesCollection.DeleteManyAsync(_ => true);

    public async Task<bool> ExistsAsync(int id) =>
        await _gamesCollection.Find(x => x.Id == id).AnyAsync();

    public async Task<List<Game>> SearchAsync(string query)
    {
        var filter = Builders<Game>.Filter.Regex("Name", new BsonRegularExpression(query, "i"));
        return await _gamesCollection.Find(filter).ToListAsync();
    }

    public async Task<StatisticsDto> GetStatisticsAsync()
    {
        var games = await _gamesCollection.Find(_ => true).ToListAsync();
        
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
