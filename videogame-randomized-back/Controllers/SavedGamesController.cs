using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Mappers;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/saved-games")]
[Authorize]
public class SavedGamesController(GamesService service, GameMapper mapper) : ControllerBase
{
    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Gets all saved games for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<GameDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GameDto>>> GetSavedGames()
    {
        var userId = GetUserId();
        var games = await service.GetByUserAsync(userId);
        return Ok(mapper.GamesToDtos(games));
    }

    /// <summary>
    /// Gets a specific saved game by ID for the current user
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameDto>> GetSavedGameById(int id)
    {
        var userId = GetUserId();
        var game = await service.GetByUserAsync(userId, id);
        if (game is null) return NotFound();
        return Ok(mapper.GameToDto(game));
    }

    /// <summary>
    /// Creates a new saved game for the current user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GameDto>> CreateSavedGame([FromBody] CreateGameDto dto)
    {
        var userId = GetUserId();

        if (await service.ExistsByUserAsync(userId, dto.Id))
        {
            return Conflict(new MessageResponseDto("Game already exists"));
        }

        var game = mapper.CreateDtoToGame(dto);
        game.UserId = userId;
        await service.CreateAsync(game);
        var gameDto = mapper.GameToDto(game);

        return CreatedAtAction(nameof(GetSavedGameById), new { id = game.Id }, gameDto);
    }

    /// <summary>
    /// Updates an existing saved game for the current user
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSavedGame(int id, [FromBody] UpdateGameDto dto)
    {
        var userId = GetUserId();
        if (!await service.UpdateSavedGameByUserAsync(userId, id, dto)) return NotFound();
        return Ok();
    }

    /// <summary>
    /// Deletes a specific saved game for the current user
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSavedGame(int id)
    {
        var userId = GetUserId();
        if (!await service.ExistsByUserAsync(userId, id)) return NotFound();
        await service.RemoveAsync(id);
        return Ok();
    }

    /// <summary>
    /// Deletes all saved games for the current user
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAllSavedGames()
    {
        var userId = GetUserId();
        await service.RemoveAllByUserAsync(userId);
        return Ok();
    }

    /// <summary>
    /// Checks if a game is saved by the current user
    /// </summary>
    [HttpGet("check/{id:int}")]
    [ProducesResponseType(typeof(IsSavedResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IsSavedResponseDto>> CheckSavedGame(int id)
    {
        var userId = GetUserId();
        var isSaved = await service.ExistsByUserAsync(userId, id);
        return Ok(new IsSavedResponseDto(isSaved));
    }

    /// <summary>
    /// Gets statistics for the current user's saved games
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(StatisticsResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatisticsResponseDto>> GetStatistics()
    {
        var userId = GetUserId();
        var stats = await service.GetStatisticsByUserAsync(userId);
        return Ok(new StatisticsResponseDto(mapper.StatsToDto(stats)));
    }

    /// <summary>
    /// Searches saved games by query string for the current user
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchGamesResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchGamesResponseDto>> SearchSavedGames([FromQuery] string q)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(q)) return Ok(new SearchGamesResponseDto([]));
        
        var games = await service.SearchByUserAsync(userId, q);
        return Ok(new SearchGamesResponseDto(mapper.GamesToDtos(games)));
    }

    /// <summary>
    /// Exports all saved games as JSON for the current user
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<FileContentResult> ExportSavedGames()
    {
        var userId = GetUserId();
        var games = await service.GetByUserAsync(userId);
        
        var dtos = mapper.GamesToDtos(games);
        
        var json = System.Text.Json.JsonSerializer.Serialize(dtos);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", "saved_games_export.json");
    }

    /// <summary>
    /// Imports a list of games for the current user
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportSavedGames([FromBody] List<CreateGameDto>? gameDtos)
    {
        var userId = GetUserId();
        if (gameDtos == null || gameDtos.Count == 0) return Ok();

        var games = gameDtos.Select(dto =>
        {
            var game = mapper.CreateDtoToGame(dto);
            game.UserId = userId;
            return game;
        }).ToList();

        await service.UpsertManyAsync(userId, games);
        return Ok();
    }
}