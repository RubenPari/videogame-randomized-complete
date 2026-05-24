using Microsoft.EntityFrameworkCore;
using videogame_randomized_back.Data;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

public class GamesService(AppDbContext db) : IGamesService
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

    public async Task<GameStatsDto> GetStatisticsByUserAsync(string userId)
    {
        var games = await db.Games
            .AsNoTracking()
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Where(g => g.UserId == userId)
            .ToListAsync();

        if (games.Count == 0)
        {
            return new GameStatsDto
            {
                TotalGames = 0,
                AverageRating = 0,
                GenreCount = new Dictionary<string, int>(),
                PlatformCount = new Dictionary<string, int>()
            };
        }

        return new GameStatsDto
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

    public async Task<bool> RemoveAsync(string userId, int id)
    {
        var game = await db.Games.FindAsync(id);
        if (game != null && game.UserId == userId)
        {
            db.Games.Remove(game);
            await db.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task UpsertManyAsync(string userId, IEnumerable<Game> games)
    {
        var gameList = games.ToList();
        if (gameList.Count == 0) return;

        var incomingIds = gameList.Select(g => g.Id).ToList();
        
        var existingGames = await db.Games
            .Where(g => g.UserId == userId && incomingIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id);

        foreach (var game in gameList)
        {
            game.UserId = userId;
            
            if (existingGames.TryGetValue(game.Id, out var existing))
            {
                existing.Note = game.Note;
                existing.PersonalRating = game.PersonalRating;
                await SyncGenresAndPlatforms(existing);
            }
            else
            {
                await SyncGenresAndPlatforms(game);
                db.Games.Add(game);
            }
        }
        
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateSavedGameByUserAsync(string userId, int id, UpdateGameDto dto)
    {
        var game = await db.Games
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

        if (game is null) return false;

        if (dto.PersonalRating.HasValue) game.PersonalRating = dto.PersonalRating;
        if (dto.Note != null) game.Note = dto.Note;

        await db.SaveChangesAsync();
        return true;
    }

    private async Task SyncGenresAndPlatforms(Game game)
    {
        var genreIds = game.Genres.Select(g => g.Id).ToHashSet();
        var platformIds = game.Platforms.Select(p => p.Id).ToHashSet();

        var existingGenres = await db.Genres
            .Where(g => genreIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id);

        var existingPlatforms = await db.Platforms
            .Where(p => platformIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var trackedGenres = db.ChangeTracker.Entries<Genre>()
            .ToDictionary(e => e.Entity.Id, e => e.Entity);

        var trackedPlatforms = db.ChangeTracker.Entries<Platform>()
            .ToDictionary(e => e.Entity.Id, e => e.Entity);

        game.Genres = game.Genres.Select(g =>
            trackedGenres.TryGetValue(g.Id, out var tracked) ? tracked :
            existingGenres.TryGetValue(g.Id, out var existing) ? existing : g
        ).ToList();

        game.Platforms = game.Platforms.Select(p =>
            trackedPlatforms.TryGetValue(p.Id, out var tracked) ? tracked :
            existingPlatforms.TryGetValue(p.Id, out var existing) ? existing : p
        ).ToList();
    }
}