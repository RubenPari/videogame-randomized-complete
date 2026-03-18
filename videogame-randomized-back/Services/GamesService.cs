using Microsoft.EntityFrameworkCore;
using videogame_randomized_back.Data;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

public class GamesService(AppDbContext db)
{
    public async Task<List<Game>> GetByUserAsync(string userId)
    {
        return await db.Games
            .AsNoTracking()
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Where(g => g.UserId == userId)
            .ToListAsync();
    }

    public async Task<Game?> GetByUserAsync(string userId, int id)
    {
        return await db.Games
            .AsNoTracking()
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
    }

    public async Task<bool> ExistsByUserAsync(string userId, int id)
    {
        return await db.Games.AnyAsync(g => g.Id == id && g.UserId == userId);
    }

    public async Task<List<Game>> SearchByUserAsync(string userId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetByUserAsync(userId);

        return await db.Games
            .AsNoTracking()
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Where(g => g.UserId == userId && EF.Functions.Like(g.Name, $"%{query}%"))
            .ToListAsync();
    }

    public async Task<StatisticsDto> GetStatisticsByUserAsync(string userId)
    {
        var games = await db.Games
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
        var gameIds = await db.Games
            .Where(g => g.UserId == userId)
            .Select(g => g.Id)
            .ToListAsync();

        if (gameIds.Count != 0)
        {
            await db.GameGenres.Where(gg => gameIds.Contains(gg.GameId)).ExecuteDeleteAsync();
            await db.GamePlatforms.Where(gp => gameIds.Contains(gp.GameId)).ExecuteDeleteAsync();
            await db.Games.Where(g => g.UserId == userId).ExecuteDeleteAsync();
        }
    }
    
    public async Task CreateAsync(Game newGame)
    {
        await SyncGenresAndPlatforms(newGame);
        db.Games.Add(newGame);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Game updatedGame)
    {
        await SyncGenresAndPlatforms(updatedGame);
        db.Games.Update(updatedGame);
        await db.SaveChangesAsync();
    }

    public async Task RemoveAsync(int id)
    {
        var game = await db.Games.FindAsync(id);
        if (game != null)
        {
            db.Games.Remove(game);
            await db.SaveChangesAsync();
        }
    }

    private async Task SyncGenresAndPlatforms(Game game)
    {
        var syncedGenres = new List<Genre>();
        foreach (var genre in game.Genres)
        {
            var tracked = db.ChangeTracker.Entries<Genre>()
                .FirstOrDefault(e => e.Entity.Id == genre.Id)?.Entity;

            if (tracked != null)
            {
                syncedGenres.Add(tracked);
            }
            else
            {
                var existing = await db.Genres.FindAsync(genre.Id);
                syncedGenres.Add(existing ?? genre);
            }
        }
        game.Genres = syncedGenres;

        var syncedPlatforms = new List<Platform>();
        foreach (var platform in game.Platforms)
        {
            var tracked = db.ChangeTracker.Entries<Platform>()
                .FirstOrDefault(e => e.Entity.Id == platform.Id)?.Entity;

            if (tracked != null)
            {
                syncedPlatforms.Add(tracked);
            }
            else
            {
                var existing = await db.Platforms.FindAsync(platform.Id);
                syncedPlatforms.Add(existing ?? platform);
            }
        }
        game.Platforms = syncedPlatforms;
    }
}