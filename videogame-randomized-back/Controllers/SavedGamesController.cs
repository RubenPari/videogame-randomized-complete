using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.Models;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/saved-games")]
public class SavedGamesController : ControllerBase
{
    private readonly SavedGamesService _service;

    public SavedGamesController(SavedGamesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var games = await _service.GetAsync();
        return Ok(new { games });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var game = await _service.GetAsync(id);
        if (game is null) return NotFound();
        return Ok(new { game });
    }

    [HttpPost]
    public async Task<IActionResult> Post(Game newGame)
    {
        // Check if exists
        var exists = await _service.ExistsAsync(newGame.Id);
        if (exists) return Conflict(new { message = "Game already exists" });

        await _service.CreateAsync(newGame);
        return CreatedAtAction(nameof(Get), new { id = newGame.Id }, newGame);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Game updatedGame)
    {
        var game = await _service.GetAsync(id);
        if (game is null) return NotFound();

        updatedGame.Id = id; // Ensure ID matches
        await _service.UpdateAsync(id, updatedGame);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var game = await _service.GetAsync(id);
        if (game is null) return NotFound();

        await _service.RemoveAsync(id);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAll()
    {
        await _service.RemoveAllAsync();
        return Ok();
    }

    [HttpGet("check/{id:int}")]
    public async Task<IActionResult> Check(int id)
    {
        var isSaved = await _service.ExistsAsync(id);
        return Ok(new { isSaved });
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _service.GetStatisticsAsync();
        return Ok(new { statistics = stats });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest();
        var games = await _service.SearchAsync(q);
        return Ok(new { games });
    }
    
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var games = await _service.GetAsync();
        // Return as a downloadable JSON file
        var json = System.Text.Json.JsonSerializer.Serialize(games);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", "saved_games_export.json");
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] List<Game> games)
    {
        if (games == null || !games.Any()) return BadRequest("No games to import");

        foreach (var game in games)
        {
            var exists = await _service.ExistsAsync(game.Id);
            if (!exists)
            {
                await _service.CreateAsync(game);
            }
            else
            {
                // Optional: Update if exists, or skip. Currently skipping or replacing?
                // Logic says import generally adds. If exists, maybe update?
                // Let's safe update (Upsert logic in loop)
                 await _service.UpdateAsync(game.Id, game);
            }
        }
        return Ok();
    }

    [HttpPost("{id:int}/note")]
    public async Task<IActionResult> AddNote(int id, [FromBody] NoteRequest request)
    {
        var game = await _service.GetAsync(id);
        if (game is null) return NotFound();

        game.Note = request.Note;
        await _service.UpdateAsync(id, game);
        return Ok();
    }

    [HttpPost("{id:int}/rating")]
    public async Task<IActionResult> AddRating(int id, [FromBody] RatingRequest request)
    {
        var game = await _service.GetAsync(id);
        if (game is null) return NotFound();

        game.PersonalRating = request.PersonalRating;
        await _service.UpdateAsync(id, game);
        return Ok();
    }
}

public class NoteRequest
{
    public string Note { get; set; } = string.Empty;
}

public class RatingRequest
{
    public int PersonalRating { get; set; }
}
