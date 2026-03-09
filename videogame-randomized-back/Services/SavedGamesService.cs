using Microsoft.EntityFrameworkCore;
using videogame_randomized_back.Data;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

public class SavedGamesService
{
    private readonly AppDbContext _db;

    public SavedGamesService(AppDbContext db)
    {
        _db = db;
    }

    // User-scoped queries

    public async Task<List<Game>> GetByUserAsync(string userId)
    {
        return await _db.Games
            .AsNoTracking()
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Where(g => g.UserId == userId)
            .ToListAsync();
    }

    public async Task<Game?> GetByUserAsync(string userId, int id)
    {
        return await _db.Games
            .AsNoTracking()
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
    }

    public async Task<bool> ExistsByUserAsync(string userId, int id)
    {
        return await _db.Games.AnyAsync(g => g.Id == id && g.UserId == userId);
    }

    public async Task<List<Game>> SearchByUserAsync(string userId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetByUserAsync(userId);

        return await _db.Games
            .AsNoTracking()
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Where(g => g.UserId == userId && EF.Functions.Like(g.Name, $"%{query}%"))
            .ToListAsync();
    }

    public async Task<StatisticsDto> GetStatisticsByUserAsync(string userId)
    {
        var games = await _db.Games
            .AsNoTracking()
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Where(g => g.UserId == userId)
            .ToListAsync();

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

        return new StatisticsDto
        {
            TotalGames = games.Count,
            AverageRating = games.Average(g => g.Rating),
            GenreCount = games
                .SelectMany(g => g.Genres)
                .GroupBy(g => g.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            PlatformCount = games
                .SelectMany(g => g.Platforms)
                .GroupBy(p => p.Name)
                .ToDictionary(p => p.Key, p => p.Count())
        };
    }

    public async Task RemoveAllByUserAsync(string userId)
    {
        var gameIds = await _db.Games
            .Where(g => g.UserId == userId)
            .Select(g => g.Id)
            .ToListAsync();

        if (gameIds.Any())
        {
            await _db.GameGenres.Where(gg => gameIds.Contains(gg.GameId)).ExecuteDeleteAsync();
            await _db.GamePlatforms.Where(gp => gameIds.Contains(gp.GameId)).ExecuteDeleteAsync();
            await _db.Games.Where(g => g.UserId == userId).ExecuteDeleteAsync();
        }
    }

    // Non-scoped write operations (game is already associated with user)

    public async Task CreateAsync(Game newGame)
    {
        await SyncGenresAndPlatforms(newGame);
        _db.Games.Add(newGame);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, Game updatedGame)
    {
        await SyncGenresAndPlatforms(updatedGame);
        _db.Games.Update(updatedGame);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(int id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game != null)
        {
            _db.Games.Remove(game);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _db.Games.AnyAsync(g => g.Id == id);
    }

    private async Task SyncGenresAndPlatforms(Game game)
    {
        foreach (var genre in game.Genres)
        {
            var existing = await _db.Genres.FindAsync(genre.Id);
            if (existing != null)
            {
                _db.Entry(genre).State = EntityState.Unchanged;
            }
            else
            {
                _db.Genres.Add(genre);
            }
        }

        foreach (var platform in game.Platforms)
        {
            var existing = await _db.Platforms.FindAsync(platform.Id);
            if (existing != null)
            {
                _db.Entry(platform).State = EntityState.Unchanged;
            }
            else
            {
                _db.Platforms.Add(platform);
            }
        }
    }
}

public class StatisticsDto
{
    public int TotalGames { get; set; }
    public double AverageRating { get; set; }
    public Dictionary<string, int> GenreCount { get; set; } = new();
    public Dictionary<string, int> PlatformCount { get; set; } = new();
}
