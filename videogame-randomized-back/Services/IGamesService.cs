using videogame_randomized_back.DTOs;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

public interface IGamesService
{
    Task<List<Game>> GetByUserAsync(string userId);
    Task<Game?> GetByUserAsync(string userId, int id);
    Task<bool> ExistsByUserAsync(string userId, int id);
    Task<List<Game>> SearchByUserAsync(string userId, string query);
    Task<GameStatsDto> GetStatisticsByUserAsync(string userId);
    Task RemoveAllByUserAsync(string userId);
    Task CreateAsync(Game newGame);
    Task<bool> RemoveAsync(string userId, int id);
    Task UpsertManyAsync(string userId, IEnumerable<Game> games);
    Task<bool> UpdateSavedGameByUserAsync(string userId, int id, UpdateGameDto dto);
}
